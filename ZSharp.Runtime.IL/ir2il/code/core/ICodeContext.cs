namespace ZSharp.Runtime.NET.IR2IL.Code
{
    public interface ICodeContext
    {
        public IL::Emit.ILGenerator IL { get; }

        public CodeStack Stack { get; }

        public IRLoader Loader { get; }
    }
}
