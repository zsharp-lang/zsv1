namespace ZSharp.CTRuntime
{
    internal class CTCodeCompiler(ZSCompiler compiler, IProcessor<CTType> processor) 
        : CodeCompiler<CTType>(compiler, processor)
    {
        public override DomainContext<CTType> CodeContext => CT;

        public override CTType Combine(params CTType[] items)
        {
            return null!;
        }
    }
}
