using ZSharp.Importer.ILLoader;

using Reified = IResult<ZSharp.HIR.Code.Expression, CommonZ.Error>;

namespace ZSharp.Compiler
{
    /// <summary>
    /// Registers how the nodes of <c>ZSharp.HIR.Code</c> are reified on the CLR.
    /// A local the code being reified declares is created by the code that does the
    /// building, so each build has its own. A local from outside is a variable of that
    /// code, so its value is what goes in the node. Unquoted code is already in the form
    /// of the building code, so it is placed as it is.
    /// </summary>
    public sealed class CodeReifiers(HIRReifier reifier, ILLoader il)
    {
        private readonly Dictionary<Code.Local, Code.Local> declared = [];

        /// <summary>
        /// The variable of the building code that holds each loop being reified.
        /// </summary>
        private readonly Dictionary<Code.Loop, Code.Local> loops = new(ReferenceEqualityComparer.Instance);

        /// <summary>
        /// Where the building code declares the variables of the blocks being reified.
        /// </summary>
        private readonly Stack<List<Code.Statement>> variables = [];

        public void Register(MappedReifier mapped)
        {
            var expressions = mapped.Expressions;
            var statements = mapped.Statements;

            expressions.Register<Code.Get>(Reify);
            // A literal is a value, so it is encoded the way any value is.
            expressions.Register<Code.StringLiteral>(literal => Ok(Call("Encode", literal)));
            expressions.Register<Code.BooleanLiteral>(literal => Ok(Call("Encode", literal)));
            expressions.Register<Code.IntegerLiteral>(literal => Ok(Call("Encode", literal)));
            expressions.Register<Code.DecimalLiteral>(literal => Ok(Call("Encode", literal)));
            expressions.Register<Code.NullLiteral>(_ => Ok(Call("Null")));
            expressions.Register<Code.NamedMember>(Reify);
            expressions.Register<Code.PositionalMember>(Reify);
            expressions.Register<Code.Set>(Reify);
            expressions.Register<Code.Cast>(Reify);
            expressions.Register<Code.Match>(Reify);
            expressions.Register<Code.ArrayLiteral>(Reify);
            expressions.Register<Code.Unquote>(unquote => Ok(unquote.Code));
            expressions.Register<Code.Call>(Reify);
            expressions.Register<Code.Index>(Reify);
            expressions.Register<Code.BlockExpression>(Reify);
            expressions.Register<Code.WhileExpression>(Reify);

            statements.Register<Code.ExpressionStatement>(Reify);
            mapped.Patterns.Register<Code.ValuePattern>(Reify);
            mapped.Patterns.Register<Code.NamePattern>(Reify);
            statements.Register<Code.Block>(Reify);
            statements.Register<Code.IfStatement>(Reify);
            statements.Register<Code.Return>(Reify);
            statements.Register<Code.Declare>(Reify);
            statements.Register<Code.While>(Reify);
            statements.Register<Code.Break>(Reify);
            statements.Register<Code.Continue>(Reify);
            statements.Register<Code.UnquoteStatement>(unquote => Ok(unquote.Code));

            var values = mapped.Values;

            values.Register<string>(value => Ok(new Code.StringLiteral(value)));
            values.Register<bool>(value => Ok(new Code.BooleanLiteral(value)));
            values.Register<int>(value => Ok(new Code.IntegerLiteral(value)));
            values.Register<double>(value => Ok(new Code.DecimalLiteral(value)));
            values.Register<HIR.Instance>(instance => Ok(new Code.Get(instance)));

            // Code that is a value is already in the form code takes, so it encodes to itself.
            values.Register<Code.Expression>(expression => Ok(expression));

            // A class's value is its type object, which stands for the class's own node.
            values.Register<Type>(type => Ok(new Code.Get(il.Load(type))));

            // Anything else is placed as an object that already exists.
            values.Register<object>(value => Ok(new Code.Get(il.Expose(value))));

            reifier.ReifyCode = Reify;

            // A statement of code that says where its statements were written is put in what
            // the code that is built says of its own, as it is built. The map is code, not an
            // object of this process, so the building code needs nothing but itself, and can
            // be written to a binary too.
            var reifyStatement = reifier.ReifyStatement;
            reifier.ReifyStatement = statement =>
                reifyStatement(statement).When(out var built).Error(out var error)
                    ? Fail(error)
                    : codes.TryPeek(out var code) && code.Maps?[statement] is { } map
                        ? Ok(Call("Mapped", new Code.Get(code.Built), built!, Call("Map",
                            new Code.StringLiteral(map.Document),
                            new Code.IntegerLiteral(map.StartLine),
                            new Code.IntegerLiteral(map.StartColumn),
                            new Code.IntegerLiteral(map.EndLine),
                            new Code.IntegerLiteral(map.EndColumn))))
                        : Ok(built!);
        }

        /// <summary>
        /// The code being reified, by what it says of where its statements were written,
        /// and the variable of the building code that holds the code it builds.
        /// </summary>
        private readonly Stack<(SourceMapping.CodeMaps? Maps, Code.Local Built)> codes = [];

        /// <summary>
        /// Code is made before its statements, in a variable of the building code, so what
        /// is said of a statement can be said as the statement is built.
        /// </summary>
        private Reified Reify(Code.Code code)
            => WithVariables(() =>
            {
                var built = new Code.Local("code", il.Load(typeof(Code.Code))) { Init = Call("Body") };
                variables.Peek().Add(new Code.Declare(built));

                codes.Push((SourceMapping.CodeMaps.Of(code), built));

                try
                {
                    if (Statements(code.Block.Statements).When(out var statements).Error(out var error))
                        return Fail(error);

                    return Ok(Chain(new Code.Get(built), statements!));
                }
                finally
                {
                    codes.Pop();
                }
            });

        /// <summary>
        /// A local of the code being reified gets a variable of its own in the code that
        /// builds it. A variable of that building code is read when the building code
        /// runs, and its value is encoded into the code being built. Any other instance is
        /// placed as it is. An overload group is built of its items, each as it is
        /// alone: an item that is a variable of the building code, as a definition being
        /// built is, is what it holds.
        /// </summary>
        private Reified Reify(Code.Get get)
        {
            if (variables.Count == 0 && Imports(get.Instance))
                return WithVariables(() => Reify(get));

            // What an import brings in is built with the code, before anything uses it.
            foreach (var member in Members(get.Instance))
                if (Imported(member).When(out _).Error(out var error))
                    return Fail(error);

            return Ok(get.Instance switch
            {
                Code.Local local when declared.TryGetValue(local, out var holder) => Call("Get", new Code.Get(holder)),
                HIR.Variable => Call("Encode", get),
                Code.ImportedMember member => Call("Get", new Code.Get(imported[member])),
                Code.OverloadGroup group when group.Items.Any(item => item is HIR.Variable or Code.ImportedMember) => Call("Get", Grouped(group)),
                _ => Call("Get", Constant(get.Instance))
            });
        }

        private Code.Expression Grouped(Code.OverloadGroup group)
            => group.Items.Aggregate<HIR.Instance, Code.Expression>(
                Call("Group"),
                (built, item) => Call("With", built, item switch
                {
                    HIR.Variable => new Code.Get(item),
                    Code.ImportedMember member => new Code.Get(imported[member]),
                    _ => Constant(item)
                })
            );

        private static IEnumerable<Code.ImportedMember> Members(HIR.Instance instance)
            => instance switch
            {
                Code.ImportedMember member => [member],
                Code.OverloadGroup group => group.Items.OfType<Code.ImportedMember>(),
                _ => []
            };

        private static bool Imports(HIR.Instance instance) => Members(instance).Any();

        /// <summary>
        /// The variables of the building code that hold the imports and imported members it
        /// builds, once for each build.
        /// </summary>
        private readonly Dictionary<HIR.Instance, Code.Local> imported = new(ReferenceEqualityComparer.Instance);

        /// <summary>
        /// An import is built with the code it is in, so each build has its own, from a
        /// source that is what the building code holds when it runs: built in a function
        /// given its source, an import is from the source given to that run. Its members
        /// are built of it, so all of them, and every use of each, share it. They are
        /// variables of the outermost code being built, which every use of them is in.
        /// </summary>
        private IResult<Code.Local, Error> Imported(Code.ImportedMember member)
        {
            if (imported.TryGetValue(member, out var known))
                return Result<Code.Local>.Ok(known);

            if (!imported.TryGetValue(member.Import, out var import))
            {
                if (Expression(member.Import.From).When(out var from).Error(out var error))
                    return Result<Code.Local>.Error(error);

                import = Outermost(new Code.Local("import", il.Load(typeof(Code.Import))) { Init = Call("Import", from!) });
                imported[member.Import] = import;
            }

            return Result<Code.Local>.Ok(imported[member] = Outermost(new Code.Local(member.Name, il.Load(typeof(Code.ImportedMember)))
            {
                Init = Call("ImportedMember", new Code.Get(import), new Code.StringLiteral(member.Name))
            }));
        }

        private Code.Local Outermost(Code.Local variable)
        {
            variables.Last().Add(new Code.Declare(variable));

            return variable;
        }

        private Reified Reify(Code.NamedMember member)
            => Expression(member.Target).When(out var target).Error(out var error)
                ? Fail(error)
                : Ok(Call("Member", target!, new Code.StringLiteral(member.Name)));

        private Reified Reify(Code.PositionalMember member)
            => Expression(member.Target).When(out var target).Error(out var error)
                ? Fail(error)
                : Ok(Call("Member", target!, Constant(member.Position)));

        private Reified Reify(Code.Set set)
        {
            if (Expression(set.Target).When(out var target).Error(out var error))
                return Fail(error);

            return Expression(set.Value).When(out var value).Error(out error)
                ? Fail(error)
                : Ok(Call("Set", target!, value!));
        }

        private Reified Reify(Code.Cast cast)
        {
            if (Expression(cast.Value).When(out var value).Error(out var error))
                return Fail(error);

            return Expression(cast.Type).When(out var type).Error(out error)
                ? Fail(error)
                : Ok(Call("Cast", value!, type!));
        }

        private Reified Reify(Code.ArrayLiteral array)
        {
            Code.Expression built = Call("Array");

            foreach (var item in array.Items)
                if (Expression(item).When(out var value).Error(out var error))
                    return Fail(error);
                else
                    built = Call("With", built, value!);

            return Ok(built);
        }

        private Reified Reify(Code.Match match)
        {
            if (Expression(match.Value).When(out var value).Error(out var error))
                return Fail(error);

            return reifier.ReifyPattern(match.Pattern).When(out var pattern).Error(out error)
                ? Fail(error)
                : Ok(Call("Match", value!, pattern!));
        }

        private Reified Reify(Code.ValuePattern pattern)
            => Expression(pattern.Value).When(out var value).Error(out var error)
                ? Fail(error)
                : Ok(Call("ValuePattern", value!));

        private Reified Reify(Code.NamePattern pattern)
        {
            if (Expression(pattern.Target).When(out var target).Error(out var error))
                return Fail(error);

            if (pattern.Type is null)
                return Ok(Call("NamePattern", target!));

            return Expression(pattern.Type).When(out var type).Error(out error)
                ? Fail(error)
                : Ok(Call("NamePattern", target!, type!));
        }

        private Reified Reify(Code.Call call)
            => Expression(call.Callee).When(out var callee).Error(out var error)
                ? Fail(error)
                : With(Call("Call", callee!), call.Arguments);

        private Reified Reify(Code.Index index)
            => Expression(index.Target).When(out var target).Error(out var error)
                ? Fail(error)
                : With(Call("Index", target!), index.Arguments);

        private Reified Reify(Code.BlockExpression block)
            => WithVariables(() =>
            {
                if (Statements(block.Statements).When(out var statements).Error(out var error))
                    return Fail(error);

                return Expression(block.Result).When(out var result).Error(out error)
                    ? Fail(error)
                    : Ok(Chain(Call("BlockExpression", result!), statements!));
            });

        private Reified Reify(Code.Block block)
            => WithVariables(() => Statements(block.Statements).When(out var statements).Error(out var error)
                ? Fail(error)
                : Ok(Chain(Call("Block"), statements!)));

        /// <summary>
        /// A local this code declares gets a variable of its own in the building code,
        /// which creates the local, and the code refers to that variable from then on.
        /// Any other definition is declared as it is.
        /// </summary>
        private Reified Reify(Code.Declare declare)
        {
            if (declare.Definition is not Code.Local local)
                return Ok(Call("Declare", Constant(declare.Definition)));

            if (variables.Count == 0)
                return WithVariables(() => Reify(declare));

            var variable = new Code.Local(local.Name, il.Load(typeof(Code.Local)))
            {
                Init = Call("Local", new Code.StringLiteral(local.Name), Constant(local.Type))
            };

            declared[local] = variable;
            variables.Peek().Add(new Code.Declare(variable));

            if (local.Init is not null)
                if (Expression(local.Init).When(out var init).Error(out var error))
                    return Fail(error);
                else
                    variables.Peek().Add(new Code.ExpressionStatement(new Code.Set
                    {
                        Target = new Code.NamedMember { Target = new Code.Get(variable), Name = nameof(Code.Local.Init) },
                        Value = init!
                    }));

            return Ok(Call("Declare", new Code.Get(variable)));
        }

        /// <summary>
        /// Reifies code whose locals the building code creates first, before the code
        /// that builds it.
        /// </summary>
        private Reified WithVariables(Func<Reified> reify)
        {
            variables.Push([]);

            try
            {
                if (reify().When(out var reified).Error(out var error))
                    return Fail(error);

                return variables.Peek() is [] ? Ok(reified!) : Ok(new Code.BlockExpression
                {
                    Statements = [.. variables.Peek()],
                    Result = reified!
                });
            }
            finally
            {
                variables.Pop();

                // What a build declared for its imports is that build's alone.
                if (variables.Count == 0)
                    imported.Clear();
            }
        }

        private IResult<List<Code.Expression>, Error> Statements(List<Code.Statement> statements)
        {
            var reified = new List<Code.Expression>();

            foreach (var statement in statements)
                if (reifier.ReifyStatement(statement).When(out var item).Error(out var error))
                    return Result<List<Code.Expression>>.Error(error);
                else
                    reified.Add(item!);

            return Result<List<Code.Expression>>.Ok(reified);
        }

        private Code.Expression Chain(Code.Expression block, List<Code.Expression> statements)
        {
            foreach (var statement in statements)
                block = Call("Add", block, statement);

            return block;
        }

        private Reified Reify(Code.ExpressionStatement statement)
            => Expression(statement.Expression).When(out var expression).Error(out var error)
                ? Fail(error)
                : Ok(Call("Statement", expression!));

        private Reified Reify(Code.Return @return)
        {
            if (@return.Value is null)
                return Ok(Call("Return"));

            return Expression(@return.Value).When(out var value).Error(out var error)
                ? Fail(error)
                : Ok(Call("Return", value!));
        }

        private Reified Reify(Code.IfStatement statement)
        {
            if (Expression(statement.Condition).When(out var condition).Error(out var error))
                return Fail(error);

            if (reifier.ReifyStatement(statement.Then).When(out var then).Error(out error))
                return Fail(error);

            if (statement.Else is null)
                return Ok(Call("If", condition!, then!));

            return reifier.ReifyStatement(statement.Else).When(out var otherwise).Error(out error)
                ? Fail(error)
                : Ok(Call("If", condition!, then!, otherwise!));
        }

        /// <summary>
        /// A loop is made by the building code before its body is built, in a variable of
        /// that code, so a break or continue in the body can name it.
        /// </summary>
        private Reified Reify(Code.While loop)
        {
            if (variables.Count == 0)
                return WithVariables(() => Reify(loop));

            var holder = Holder(loop, typeof(Code.While), "While");

            if (Expression(loop.Condition).When(out var condition).Error(out var error))
                return Fail(error);

            if (reifier.ReifyStatement(loop.Body).When(out var body).Error(out error))
                return Fail(error);

            if (loop.Else is null)
                return Ok(Call("Loop", new Code.Get(holder), condition!, body!));

            return reifier.ReifyStatement(loop.Else).When(out var otherwise).Error(out error)
                ? Fail(error)
                : Ok(Call("Loop", new Code.Get(holder), condition!, body!, otherwise!));
        }

        private Reified Reify(Code.WhileExpression loop)
        {
            if (variables.Count == 0)
                return WithVariables(() => Reify(loop));

            var holder = Holder(loop, typeof(Code.WhileExpression), "WhileExpression");

            if (Expression(loop.Condition).When(out var condition).Error(out var error))
                return Fail(error);

            if (reifier.ReifyStatement(loop.Body).When(out var body).Error(out error))
                return Fail(error);

            return Expression(loop.Else).When(out var otherwise).Error(out error)
                ? Fail(error)
                : Ok(Call("Loop", new Code.Get(holder), condition!, body!, otherwise!));
        }

        private Code.Local Holder(Code.Loop loop, Type type, string make)
        {
            var holder = new Code.Local("loop", il.Load(type)) { Init = Call(make) };

            loops[loop] = holder;
            variables.Peek().Add(new Code.Declare(holder));

            return holder;
        }

        private Reified Reify(Code.Break leave)
        {
            if (!loops.TryGetValue(leave.Loop, out var holder))
                return Fail(new ErrorMessage("A break leaves a loop outside the code being reified."));

            if (leave.Value is null)
                return Ok(Call("Break", new Code.Get(holder)));

            return Expression(leave.Value).When(out var value).Error(out var error)
                ? Fail(error)
                : Ok(Call("Break", new Code.Get(holder), value!));
        }

        private Reified Reify(Code.Continue next)
            => loops.TryGetValue(next.Loop, out var holder)
                ? Ok(Call("Continue", new Code.Get(holder)))
                : Fail(new ErrorMessage("A continue names a loop outside the code being reified."));

        private Reified With(Code.Expression target, List<Code.Argument> arguments)
        {
            foreach (var argument in arguments)
                if (Expression(argument.Value).When(out var value).Error(out var error))
                    return Fail(error);
                else
                    target = argument.Name is null
                        ? Call("With", target, value!)
                        : Call("With", target, value!, new Code.StringLiteral(argument.Name));

            return Ok(target);
        }

        private Reified Expression(Code.Expression expression)
            => reifier.ReifyExpression(expression);

        private Code.Call Call(string node, params Code.Expression[] arguments)
            => new()
            {
                Callee = new Code.NamedMember { Target = new Code.Get(il.Load(typeof(Nodes))), Name = node },
                Arguments = [.. arguments.Select(argument => new Code.Argument { Value = argument })]
            };

        private Code.Get Constant(object value)
            => new(il.Expose(value));

        private static Reified Ok(Code.Expression expression)
            => Result<Code.Expression>.Ok(expression);

        private static Reified Fail(Error error)
            => Result<Code.Expression>.Error(error);
    }
}
