using ZSharp.Importer.ILLoader;

using Reified = IResult<ZSharp.HIR.Code.Expression, CommonZ.Error>;

namespace ZSharp.Compiler
{
    /// <summary>
    /// Registers how the nodes of <c>ZSharp.HIR.Code</c> are reified on the CLR.
    /// A local declared by the code being reified is structure, so it is placed as
    /// it is. A local from outside is a variable of the code that does the building,
    /// so its value is what goes in the node.
    /// </summary>
    public sealed class CodeReifiers(HIRReifier reifier, ILLoader il)
    {
        private readonly HashSet<Code.Local> declared = [];

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
            => Ok(Call("Get", get.Instance is Code.Local local && !declared.Contains(local)
                ? get
                : Constant(get.Instance)
            ));

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
            declared.UnionWith(block.Locals);

            return Expression(block.Result).When(out var result).Error(out var error)
                ? Fail(error)
                : Fill(Call("BlockExpression", result!), block.Locals, block.Statements);
        }

        private Reified Reify(Code.Block block)
        {
            declared.UnionWith(block.Locals);

            return Fill(Call("Block"), block.Locals, block.Statements);
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

        private Reified Fill(Code.Expression block, List<Code.Local> locals, List<Code.Statement> statements)
        {
            foreach (var local in locals)
                block = Call("Declare", block, Constant(local));

            foreach (var statement in statements)
                if (reifier.ReifyStatement(statement).When(out var reified).Error(out var error))
                    return Fail(error);
                else
                    block = Call("Add", block, reified!);

            return Ok(block);
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
