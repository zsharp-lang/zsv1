using ZSharp.IR;

namespace ZSharp.VM
{
    public sealed class ZSFunction(Function function, ZSObject type) 
        : ZSIRObject<Function>(function, 0, type)
    {
        public Instruction[] Code { get; set; } = [];

        public int ArgumentCount { get; set; } = 0;

        public int StackSize { get; set; } = 0;

        public int LocalCount { get; set; } = 0;

        public static ZSFunction CreateFrom(Function function, Instruction[] code, ZSObject type)
            => new(function, type)
            {
                ArgumentCount = function.Signature.Length,
                Code = code,
                LocalCount = function.HasBody && function.Body.HasLocals ? function.Body.Locals.Count : 0,
                StackSize = function.HasBody ? function.Body.StackSize : 0
            };
    }
}
