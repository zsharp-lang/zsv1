using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public interface IGenericInstantiable
        : CompilerObject
    {
        public CompilerObjectResult Instantiate(
            Compiler.Compiler compiler,
            Argument[] arguments
        );
    }
}
