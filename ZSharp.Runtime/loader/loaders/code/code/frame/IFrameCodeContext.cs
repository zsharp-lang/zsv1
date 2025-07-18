namespace ZSharp.Runtime.Loaders
{
    public interface IFrameCodeContext
        : ICodeContext
    {
        public Parameter GetParameter(IR.Parameter parameter);

        public Local GetLocal(IR.VM.Local local);
    }
}
