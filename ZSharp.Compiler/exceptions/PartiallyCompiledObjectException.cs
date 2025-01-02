namespace ZSharp.Compiler
{
    public sealed class PartiallyCompiledObjectException(
        CompilerObject @object, 
        string? message = null
    )
        : CompilerObjectException(@object, message)
    {

    }
}
