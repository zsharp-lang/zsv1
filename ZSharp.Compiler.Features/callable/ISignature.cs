namespace ZSharp.Compiler.Features.Callable
{
    public interface ISignature
    {
        public IEnumerable<CompilerObject> Parameters(Compiler compiler);
    }
}
