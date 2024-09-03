using ZSharp.IR;

namespace ZSharp.VM
{
    public sealed class ZSFunction(Function function, int fieldCount, ZSObject type) 
        : ZSIRObject<Function>(function, fieldCount, type)
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

        public ZSFunction(Function function, ZSObject type)
            : this(function, 0, type) { }

        public static ZSFunction CreateFrom(Function function, Instruction[] code, ZSObject type)
            => CreateFrom(function, code, type, null!);

        public static ZSFunction CreateFrom(Function function, Instruction[] code, ZSObject type, ZSObject self)
            => new(function, self is null ? 0 : 1, type)
            {
                ArgumentCount = function.Signature.Length,
                Code = code,
                LocalCount = function.HasBody && function.Body.HasLocals ? function.Body.Locals.Count : 0,
                Self = self,
                StackSize = function.HasBody ? function.Body.StackSize : 0
            };
    }
}
