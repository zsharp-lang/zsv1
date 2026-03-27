namespace ZSharp.Platform.Runtime
{
    internal sealed class TransformCall(IR.ICallable callable) : IR.VM.Instruction
    {
        public IR.ICallable Callable { get; } = callable;
    }
}
