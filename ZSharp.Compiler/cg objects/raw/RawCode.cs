using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class RawCode(IRCode code)
        : CompilerObject
        , ICTReadable
    {
        private readonly IRCode code = code;

        public IRCode Code => code;

        public IType Type => code.RequireValueType();

        public IRCode Read(Compiler.Compiler _)
            => code;

        IType IDynamicallyTyped.GetType(Compiler.Compiler compiler)
            => code.IsVoid ? compiler.TypeSystem.Void : code.RequireValueType();
    }
}
