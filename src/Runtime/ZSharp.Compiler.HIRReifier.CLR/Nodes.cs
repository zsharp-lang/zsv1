namespace ZSharp.Compiler
{
    /// <summary>
    /// What reified code calls on the CLR to build <c>ZSharp.HIR.Code</c> nodes. It
    /// is a set of plain functions so that reified code is just calls, and lists are
    /// filled by chaining because code has no list literal.
    /// </summary>
    public static class Nodes
    {
        /// <summary>
        /// Refers to a value of the code doing the building. A definition or a parameter
        /// is HIR already. Anything else is a value that code captured.
        /// </summary>
        public static Code.Get Get(object value)
            => new(value as HIR.Instance ?? Core.Runtime.ModuleScope.ILLoader.Expose(value));

        public static Code.Local Local(string name, HIR.Type type) => new(name, type);

        public static Code.TypeExpression TypeExpression(Code.Expression expression) => new(expression);

        public static Code.Code CodeOf(Code.Statement statement)
        {
            var code = new Code.Code();

            code.Block.Statements.Add(statement);

            return code;
        }

        public static Code.StringLiteral String(string value) => new(value);

        public static Code.BooleanLiteral Boolean(bool value) => new(value);

        public static Code.NamedMember Member(Code.Expression target, string name)
            => new() { Target = target, Name = name };

        public static Code.PositionalMember Member(Code.Expression target, int position)
            => new() { Target = target, Position = position };

        public static Code.Set Set(Code.Expression target, Code.Expression value)
            => new() { Target = target, Value = value };

        public static Code.Cast Cast(Code.Expression value, Code.Expression type)
            => new() { Value = value, Type = type };

        public static Code.Call Call(Code.Expression callee) => new() { Callee = callee };

        public static Code.Call With(Code.Call call, Code.Expression value)
            => With(call, value, null);

        public static Code.Call With(Code.Call call, Code.Expression value, string? name)
        {
            call.Arguments.Add(new() { Value = value, Name = name });

            return call;
        }

        public static Code.Index Index(Code.Expression target) => new() { Target = target };

        public static Code.Index With(Code.Index index, Code.Expression value)
            => With(index, value, null);

        public static Code.Index With(Code.Index index, Code.Expression value, string? name)
        {
            index.Arguments.Add(new() { Value = value, Name = name });

            return index;
        }

        public static Code.BlockExpression BlockExpression(Code.Expression result)
            => new() { Result = result };

        public static Code.BlockExpression Declare(Code.BlockExpression block, Code.Local local)
        {
            block.Locals.Add(local);

            return block;
        }

        public static Code.BlockExpression Add(Code.BlockExpression block, Code.Statement statement)
        {
            block.Statements.Add(statement);

            return block;
        }

        public static Code.Block Block() => new();

        public static Code.Block Declare(Code.Block block, Code.Local local)
        {
            block.Locals.Add(local);

            return block;
        }

        public static Code.Block Add(Code.Block block, Code.Statement statement)
        {
            block.Statements.Add(statement);

            return block;
        }

        public static Code.ExpressionStatement Statement(Code.Expression expression) => new(expression);

        public static Code.Return Return() => new();

        public static Code.Return Return(Code.Expression value) => new() { Value = value };

        public static Code.IfStatement If(Code.Expression condition, Code.Statement then)
            => new() { Condition = condition, Then = then };

        public static Code.IfStatement If(Code.Expression condition, Code.Statement then, Code.Statement otherwise)
            => new() { Condition = condition, Then = then, Else = otherwise };
    }
}
