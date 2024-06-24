namespace ZSharp.CTRuntime
{
    internal interface IDefinitionCompiler<T>
    {
        IBinding<T> Compile(RAST.RDefinition definition);
    }
}
