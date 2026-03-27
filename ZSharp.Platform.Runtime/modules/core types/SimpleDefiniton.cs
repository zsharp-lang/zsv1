using ZSharp.IR;

namespace ZSharp.Platform.Runtime.Modules.CoreTypes
{
    public sealed class SimpleDefiniton(Class definition)
    {
        public TypeDefinition Definition { get; } = definition;

        public IType Reference => new ClassReference(definition);
    }
}
