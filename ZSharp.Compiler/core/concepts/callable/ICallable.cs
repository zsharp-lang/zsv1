namespace ZSharp.Compiler
{
    /// <summary>
    /// Should be implemented by any binding that is callable.
    /// </summary>
    public interface ICTCallable_Old
        : ICTCallable
    {
        CompilerObjectResult ICTCallable.Call(Compiler compiler, Argument_NEW<CompilerObject>[] arguments)
            => CompilerObjectResult.Ok(
                Call(compiler, arguments.Select(arg => new Argument(arg.Name, arg.Value)).ToArray())
            );

        public CompilerObject Call(Compiler compiler, Argument[] arguments);
    }
}
