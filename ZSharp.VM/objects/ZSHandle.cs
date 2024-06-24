namespace ZSharp.VM
{
    public class ZSHandle(object @object) : ZSObject(TypeSystem.Any)
    {
        public object Object { get; } = @object;
    }

    public sealed class ZSHandle<T>(T @object)
        : ZSHandle(@object)
        where T : notnull
    {
        public new T Object => (T)base.Object;
    }
}
