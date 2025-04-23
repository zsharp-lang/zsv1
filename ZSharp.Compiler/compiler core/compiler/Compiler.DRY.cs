namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        public CompilerObjectResult Wrap(Func<Compiler, CompilerObject> func)
        {
            try
            {
                return CompilerObjectResult.Ok(func(this));
            } catch (CompilerObjectException e)
            {
                return CompilerObjectResult.Error(e.Message);
            }
        }
    }
}
