using ZSharp.Importer.ILLoader;

using Reified = IResult<ZSharp.HIR.Code.Expression, CommonZ.Error>;

namespace ZSharp.Compiler
{
    /// <summary>
    /// Registers how the nodes of <c>ZSharp.HIR.Code</c> are reified on the CLR.
    /// A local declared by the code being reified is created by the code that does the
    /// building, so each build has its own. A local from outside is a variable of that
    /// code, so its value is what goes in the node.
    /// </summary>
    public sealed class CodeReifiers(HIRReifier reifier, ILLoader il)
    {
        private readonly Dictionary<Code.Local, Code.Local> declared = [];

        public void Register(MappedReifier mapped)
        {
            var expressions = mapped.Expressions;
            var statements = mapped.Statements;

            expressions.Register<Code.Get>(Reify);
            expressions.Register<Code.StringLiteral>(literal => Ok(Call("String", literal)));
            expressions.Register<Code.BooleanLiteral>(literal => Ok(Call("Boolean", literal)));
            expressions.Register<Code.NamedMember>(Reify);
            expressions.Register<Code.PositionalMember>(Reify);
            expressions.Register<Code.Set>(Reify);
            expressions.Register<Code.Cast>(Reify);
            expressions.Register<Code.Call>(Reify);
            expressions.Register<Code.Index>(Reify);
            expressions.Register<Code.BlockExpression>(Reify);
            expressions.Register<Code.Escape>(escape => Ok(Call("Get", escape.Value)));

            statements.Register<Code.ExpressionStatement>(Reify);
            statements.Register<Code.Block>(Reify);
            statements.Register<Code.IfStatement>(Reify);
            statements.Register<Code.Return>(Reify);
        }

        private Reified Reify(Code.Get get)
            => Ok(Call("Get", get.Instance switch
            {
                Code.Local local when declared.TryGetValue(local, out var holder) => new Code.Get(holder),
                Code.Local => get,
                _ => Constant(get.Instance)
            }));

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
        {
            if (Declare(block.Locals).When(out var holders).Error(out var error))
                return Fail(error);

            return Expression(block.Result).When(out var result).Error(out error)
                ? Fail(error)
                : Fill(Call("BlockExpression", result!), block.Locals, block.Statements, holders!);
        }

        private Reified Reify(Code.Block block)
            => Declare(block.Locals).When(out var holders).Error(out var error)
                ? Fail(error)
                : Fill(Call("Block"), block.Locals, block.Statements, holders!);

        /// <summary>
        /// Gives each local a variable in the code that does the building.
        /// </summary>
        private IResult<List<Code.Statement>, Error> Declare(List<Code.Local> locals)
        {
            var holders = new List<Code.Statement>();

            foreach (var local in locals)
            {
                if (TypeOf(local).When(out var type).Error(out var error))
                    return Result<List<Code.Statement>>.Error(error);

                var holder = new Code.Local(local.Name, il.Load(typeof(Code.Local)));

                declared[local] = holder;

                holders.Add(new Code.ExpressionStatement(new Code.Set
                {
                    Target = new Code.Get(holder),
                    Value = Call("Local", new Code.StringLiteral(local.Name), type!)
                }));
            }

            return Result<List<Code.Statement>>.Ok(holders);
        }

        private Reified TypeOf(Code.Local local)
            => local.Type is Code.TypeExpression type
                ? Expression(type.Expression).When(out var expression).Error(out var error)
                    ? Fail(error)
                    : Ok(Call("TypeExpression", expression!))
                : Ok(Constant(local.Type));

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

        private Reified Fill(
            Code.Expression block,
            List<Code.Local> locals,
            List<Code.Statement> statements,
            List<Code.Statement> holders
        )
        {
            foreach (var local in locals)
                block = Call("Declare", block, new Code.Get(declared[local]));

            foreach (var statement in statements)
                if (reifier.ReifyStatement(statement).When(out var reified).Error(out var error))
                    return Fail(error);
                else
                    block = Call("Add", block, reified!);

            if (locals.Count == 0)
                return Ok(block);

            var result = new Code.BlockExpression { Result = block, Statements = holders };

            result.Locals.AddRange(locals.Select(local => declared[local]));

            return Ok(result);
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
