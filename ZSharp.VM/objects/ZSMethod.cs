using ZSharp.IR;

namespace ZSharp.VM
{
    public sealed class ZSMethod(Method method, int fieldCount, ZSObject type, ZSFunction function)
        : ZSIRObject<Method>(method, fieldCount, type)
    {
        public ZSObject? Self
        {
            get => ObjectSize == 0 ? null : GetField(0);
            set
            {
                if (ObjectSize == 0)
                    if (value is null) return;
                    else throw new Exception("Function does not have a self object!");
                SetField(0, value!);
            }
        }

        public ZSFunction Function { get; } = function;

        public ZSMethod(Method method, ZSFunction function, ZSObject type)
            : this(method, 0, type, function) { }

        public static ZSMethod CreateFrom(Method method, ZSFunction function, ZSObject type)
            => CreateFrom(method, function, type, null!);

        public static ZSMethod CreateFrom(Method method, ZSFunction function, ZSObject type, ZSObject self)
            => new(method, self is null ? 0 : 1, type, function)
            {
                Self = self,
            };
    }
}
