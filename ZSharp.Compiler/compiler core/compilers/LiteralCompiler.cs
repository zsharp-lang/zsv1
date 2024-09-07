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
                _ => throw new NotImplementedException(),
            };
    }
}
