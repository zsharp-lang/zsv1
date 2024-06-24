namespace ZSharp.CTRuntime
{
    internal class RTCodeCompiler(ZSCompiler compiler, IProcessor<RTType> processor) 
        : CodeCompiler<RTType>(compiler, processor)
    {
        public override DomainContext<RTType> CodeContext => RT;

        public override RTType Combine(params RTType[] items)
        {
            CodeCombiner combiner = new();

            foreach (var item in items)
                combiner.Add(item);

            return combiner.Create();
        }
    }
}
