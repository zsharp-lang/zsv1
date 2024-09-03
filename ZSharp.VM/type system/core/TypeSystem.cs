using ZSharp.IR;

namespace ZSharp.VM
{
    public sealed class TypeSystem
    {
        public ZSObject Type { get; }
        
        public ZSObject Boolean { get; }
        
        public ZSObject Function { get; }
        
        public ZSObject Module { get; }
        
        public ZSObject Null { get; }
        
        public ZSObject String { get; }

        public ZSObject Void { get; }

        public TypeSystem(RuntimeModule runtimeModule)
            : this(runtimeModule.TypeSystem) { }

        public TypeSystem(IR.TypeSystem typeSystem)
        {
            Type = new Types.Type(typeSystem.Type);

            Boolean = CreatePrimitive(null!);
            Function = CreatePrimitive(null!);
            Module = CreatePrimitive(null!);
            Null = CreatePrimitive(null!);
            String = CreatePrimitive(typeSystem.String);
            Void = CreatePrimitive(typeSystem.Void);
        }

        private ZSObject CreatePrimitive(Class @class)
            => new Types.PrimitiveType(@class, Type);
    }
}
