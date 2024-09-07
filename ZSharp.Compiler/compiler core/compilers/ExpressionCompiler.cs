using ZSharp.CGRuntime;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    public sealed class ExpressionCompiler(Compiler compiler)
        : CompilerBase(compiler)
    {
        public CGObject Compile(RExpression expression)
            => expression switch
            {
                RBinaryExpression binary => Compile(binary),
                RCall call => Compile(call),
                RDefinition definition => Compiler.CompileNode(definition),
                RId id => Context.CurrentScope.Cache(id.Name) ?? throw new("Name not found " + id.Name),
                RLiteral literal => Compiler.CompileNode(literal),
                _ => throw new NotImplementedException(),
            };

        private CGObject Compile(RBinaryExpression binary)
        {
            var left = Compiler.CompileNode(binary.Left);

            // TODO: operator . should be implemented as a CGF
            if (binary.Operator == ".")
            {
                if (binary.Right is not RLiteral literal)
                    throw new();

                if (literal.Type == RLiteralType.String)
                    return Compiler.Member(left, literal.As<string>());
                else if (literal.Type == RLiteralType.Integer)
                    return Compiler.Member(left, literal.As<int>());
                else throw new();
            }

            // TODO: operator lookup table
            throw new NotImplementedException();
        }

        private CGObject Compile(RCall call)
        {
            List<Argument> arguments = [];

            foreach (var argument in call.Arguments)
                arguments.Add(new(argument.Name, Compiler.CompileNode(argument.Value)));

            var callee = Compiler.CompileNode(call.Callee);

            return Compiler.Call(callee, [.. arguments]);
        }
    }
}
