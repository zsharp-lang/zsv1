using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public sealed class RawCode(Code code)
        : CGObject
        , ICTReadable
    {
        private readonly Code code = code;

        public Code Code => code;

        public CGObject Type => code.RequireValueType();

        public Code Read(Compiler.Compiler _)
            => code;
    }
}
