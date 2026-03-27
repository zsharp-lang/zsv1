namespace ZSharp.Platform.Runtime.Loaders
{
    public delegate void InstructionTransformer(
        ICodeContext context,
        IR.VM.Instruction instruction
    );

    internal interface ITransformableCodeContext
        : ICodeContext
    {
        public InstructionTransformer GetTransformer(TransformCall instruction);
    }
}
