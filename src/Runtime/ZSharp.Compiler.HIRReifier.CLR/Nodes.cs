namespace ZSharp.Compiler
{
    /// <summary>
    /// What reified code calls on the CLR to build <c>ZSharp.HIR.Code</c> nodes. It
    /// is a set of plain functions so that reified code is just calls, and lists are
    /// filled by chaining because code has no list literal.
    /// </summary>
    public static class Nodes
    {
        public static Code.Get Get(HIR.Instance instance) => new(instance);

        /// <summary>
        /// Encodes a value held by the code doing the building into the code it builds.
        /// </summary>
        public static Code.Expression Encode(object value)
            => Core.Compiler.ModuleScope.HIRReifier.EncodeValue(value).When(out var encoded).Error(out var error)
                ? throw new InvalidOperationException(error.ToString())
                : encoded!;

        public static Code.Local Local(string name, HIR.Type type) => new(name, type);


        public static Code.Declare Declare(HIR.Definition definition) => new(definition);

        public static Code.Code CodeOf(Code.Statement statement)
        {
            var code = new Code.Code();

            code.Block.Statements.Add(statement);

            return code;
        }



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

        public static Code.BlockExpression Add(Code.BlockExpression block, Code.Statement statement)
        {
            block.Statements.Add(statement);

            return block;
        }

        public static Code.Block Block() => new();

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

        /// <summary>
        /// A loop, made before its body so that a break in the body can name it.
        /// </summary>
        public static Code.While While() => new();

        public static Code.WhileExpression WhileExpression() => new();

        public static Code.While Loop(Code.While loop, Code.Expression condition, Code.Statement body)
        {
            loop.Condition = condition;
            loop.Body = body;
            return loop;
        }

        public static Code.While Loop(Code.While loop, Code.Expression condition, Code.Statement body, Code.Statement otherwise)
        {
            loop.Else = otherwise;
            return Loop(loop, condition, body);
        }

        public static Code.WhileExpression Loop(Code.WhileExpression loop, Code.Expression condition, Code.Statement body, Code.Expression otherwise)
        {
            loop.Condition = condition;
            loop.Body = body;
            loop.Else = otherwise;
            return loop;
        }

        public static Code.Break Break(Code.Loop loop) => new(loop);

        public static Code.Break Break(Code.Loop loop, Code.Expression value) => new(loop) { Value = value };

        public static Code.Continue Continue(Code.Loop loop) => new(loop);
    }
}
