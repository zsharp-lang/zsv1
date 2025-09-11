namespace ZSharp.SourceCompiler.Module.Objects
{
    partial class Function
        : ICTCallable
    {
        Result<CompilerObject> ICTCallable.Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            throw new NotImplementedException();
        }
    }
}
