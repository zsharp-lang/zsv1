using ZSharp.IR;

namespace ZSharp.VM
{
    public sealed class ZSMethod(Method method, int fieldCount, ZSObject type)
        : ZSIRObject<Method>(method, fieldCount, type)
    {
        public Instruction[] Code { get; set; } = [];

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

        public int ArgumentCount { get; set; } = 0;

        public int StackSize { get; set; } = 0;

        public int LocalCount { get; set; } = 0;

        public ZSMethod(Method method, ZSObject type)
            : this(method, 0, type) { }

        public static ZSMethod CreateFrom(Method method, Instruction[] code, ZSObject type)
            => CreateFrom(method, code, type, null!);

        public static ZSMethod CreateFrom(Method method, Instruction[] code, ZSObject type, ZSObject self)
            => new(method, self is null ? 0 : 1, type)
            {
                ArgumentCount = method.Signature.Length,
                Code = code,
                LocalCount = method.HasBody && method.Body.HasLocals ? method.Body.Locals.Count : 0,
                Self = self,
                StackSize = method.HasBody ? method.Body.StackSize : 0
            };
    }
}
