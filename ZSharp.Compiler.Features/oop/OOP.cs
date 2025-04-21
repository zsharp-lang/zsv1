using ZSharp.Objects;

namespace ZSharp.Compiler
{
    public sealed class OOP(Compiler compiler) : Feature(compiler)
    {
        public bool IsClass(CompilerObject @object)
        {
            throw new NotImplementedException();
        }

        public CompilerObject BaseOf(CompilerObject @object)
        {
            throw new NotImplementedException();
        }

        public CompilerObject[] InterfacesOf(CompilerObject @object)
        {
            throw new NotImplementedException();
        }
    }
}
