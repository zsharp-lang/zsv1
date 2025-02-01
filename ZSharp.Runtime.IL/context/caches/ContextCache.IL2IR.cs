using CommonZ.Utils;

namespace ZSharp.Runtime.NET
{
    internal sealed class ContextCacheIL2IR
    {
        public Cache<Type, IR.IType> Types { get; } = [];

        public Cache<IL.Module, IR.Module> Modules { get; } = [];

        public Cache<Type, IR.OOPType> OOPTypes { get; } = [];

        public Cache<IL.MethodBase, IR.ICallable> Callables { get; } = [];

        public Cache<IL.FieldInfo, IR.Field> Fields { get; } = [];

        public Cache<IL.FieldInfo, IR.Global> Globals { get; } = [];
    }
}
