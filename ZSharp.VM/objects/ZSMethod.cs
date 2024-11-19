using ZSharp.IR;

namespace ZSharp.VM
{
    public sealed class ZSMethod(Method method, ZSObject type)
        : ZSIRObject<Method>(method, method.IsStatic ? 0 : 1, type)
        , ICallable
    {
        public ZSObject? Self
        {
            get => ObjectSize == 0 ? null : GetField(0);
            set
            {
                if (ObjectSize == 0)
                    if (value is null) return;
                    else throw new Exception("Static method does not have a self object!");
                SetField(0, value!);
            }
        }

        public Instruction[] Code { get; set; } = [];

        public int ArgumentCount { get; set; } = 0;

        public int StackSize { get; set; } = 0;

        public int LocalCount { get; set; } = 0;

        public static ZSMethod CreateFrom(Method method, ZSObject type)
            => new(method, type)
            {
                ArgumentCount = method.Signature.Length,
                LocalCount = method.HasBody && method.Body.HasLocals ? method.Body.Locals.Count : 0,
                StackSize = method.HasBody ? method.Body.StackSize : 0
            };

        public static ZSMethod CreateFrom(Method method, ZSObject type, ZSObject self)
            => new(method, type)
            {
                ArgumentCount = method.Signature.Length,
                LocalCount = method.HasBody && method.Body.HasLocals ? method.Body.Locals.Count : 0,
                Self = self,
                StackSize = method.HasBody ? method.Body.StackSize : 0
            };

        public static ZSMethod CreateFrom(Method method, Instruction[] code, ZSObject type)
            => new(method, type)
            {
                ArgumentCount = method.Signature.Length,
                Code = code,
                LocalCount = method.HasBody && method.Body.HasLocals ? method.Body.Locals.Count : 0,
                StackSize = method.HasBody ? method.Body.StackSize : 0
            };

        public static ZSMethod CreateFrom(Method method, Instruction[] code, ZSObject type, ZSObject self)
            => new(method, type)
            {
                ArgumentCount = method.Signature.Length,
                Code = code,
                LocalCount = method.HasBody && method.Body.HasLocals ? method.Body.Locals.Count : 0,
                StackSize = method.HasBody ? method.Body.StackSize : 0,
                Self = self
            };
    }
}
