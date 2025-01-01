using CommonZ.Utils;

namespace ZSharp.Runtime.NET
{
    internal sealed class ContextCacheIR2IL
    {
        public Cache<IR.Module, IL.Module> Modules { get; } = [];

        public Cache<IR.IType, Type> Types { get; } = [];

        public Cache<IR.ICallable, IL.MethodBase> Callables { get; } = [];

        public Cache<IR.Field, IL.FieldInfo> Fields { get; } = [];

        public Cache<IR.Global, IL.FieldInfo> Globals { get; } = [];
    }
}
