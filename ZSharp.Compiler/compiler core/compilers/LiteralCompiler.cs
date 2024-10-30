using ZSharp.RAST;

namespace ZSharp.Compiler
{
    public sealed class LiteralCompiler(Compiler compiler)
        : CompilerBase(compiler)
    {
        public CGObject Compile(RLiteral literal)
            => literal.Type switch
            {
                RLiteralType.String => Compiler.CreateString(literal.As<string>()),
                RLiteralType.Integer => Compiler.CreateInteger(literal.As<DefaultIntegerType>()),
                RLiteralType.Real => Compiler.CreateFloat32(literal.As<float>()), // TODO: configurable default float
                _ => throw new NotImplementedException(),
            };
    }
}
