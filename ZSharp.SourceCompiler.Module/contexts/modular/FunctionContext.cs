namespace ZSharp.SourceCompiler.Module.Contexts
{
    internal sealed class FunctionContext(Objects.Function function)
        //: IContext
        //, IObjectContext<Objects.Function>
    {
        public IContext? Parent { get; set; }

        public Objects.Function Object { get; } = function;
    }
}
