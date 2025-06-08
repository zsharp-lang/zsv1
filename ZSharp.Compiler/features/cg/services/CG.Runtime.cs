using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Compiler
{
    public partial struct CG
    {
        public CompilerObject? RuntimeDescriptor(CompilerObject @object)
            => @object is IHasRuntimeDescriptor hasRuntimeDescriptor
            ? hasRuntimeDescriptor.GetRuntimeDescriptor(compiler)
            : null;

        public bool RuntimeDescriptor(CompilerObject @object, [NotNullWhen(true)] out CompilerObject? rt)
            => (rt = RuntimeDescriptor(@object)) is not null;
    }
}
