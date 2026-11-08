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
            expressions.Register<Code.NamedMember>(Reify);
            expressions.Register<Code.PositionalMember>(Reify);
            expressions.Register<Code.Set>(Reify);
            expressions.Register<Code.Cast>(Reify);
            expressions.Register<Code.Unquote>(unquote => Ok(unquote.Code));
            expressions.Register<Code.Call>(Reify);
            expressions.Register<Code.Index>(Reify);
            expressions.Register<Code.BlockExpression>(Reify);

            statements.Register<Code.ExpressionStatement>(Reify);
            statements.Register<Code.Block>(Reify);
            statements.Register<Code.IfStatement>(Reify);
            statements.Register<Code.Return>(Reify);
            statements.Register<Code.Declare>(Reify);
            statements.Register<Code.UnquoteStatement>(unquote => Ok(unquote.Code));

            var values = mapped.Values;

            values.Register<string>(value => Ok(new Code.StringLiteral(value)));
            values.Register<bool>(value => Ok(new Code.BooleanLiteral(value)));
            values.Register<HIR.Instance>(instance => Ok(new Code.Get(instance)));

            // A class's value is its type object, which stands for the class's own node.
            values.Register<Type>(type => Ok(new Code.Get(il.Load(type))));

            // Anything else is placed as an object that already exists.
            values.Register<object>(value => Ok(new Code.Get(il.Expose(value))));
        }

        /// <summary>
        /// A local of the code being reified gets a variable of its own in the code that
        /// builds it. A variable of that building code is read when the building code
        /// runs, and its value is encoded into the code being built. Any other instance is
        /// placed as it is.
        /// </summary>
        private Reified Reify(Code.Get get)
            => Ok(get.Instance switch
            {
                Code.Local local when declared.TryGetValue(local, out var holder) => Call("Get", new Code.Get(holder)),
                Code.Local => Call("Encode", get),
                _ => Call("Get", Constant(get.Instance))
            });

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
