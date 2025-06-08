namespace ZSharp.Compiler
{
    public interface ICTGetIndex : ICTGetIndex_NEW
    {
        CompilerObjectResult ICTGetIndex_NEW.Index(Compiler compiler, Argument_NEW<CompilerObject>[] arguments)
            => CompilerObjectResult.Ok(Index(compiler, [.. arguments.Select(arg => new Argument(arg.Name, arg.Value))]));

        public CompilerObject Index(Compiler compiler, Argument[] index);
    }
}
