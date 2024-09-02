using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public sealed class RawCode(Code code)
        : CGObject
        , ICTReadable
    {
        private readonly Code code = code;

        public Code Code => code;

        public IR.IType Type => code.RequireValueType();

        public Code Read(ICompiler _)
            => code;
    }
}
