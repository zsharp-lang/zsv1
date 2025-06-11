using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class RawCode(IRCode code)
        : CompilerObject
        , ICTReadable
        ,ICTTypeCast
        , ICTCompileIRCode
    {
        private readonly IRCode code = code;

        public IRCode Code => code;

        public IType Type => code.RequireValueType();

        public IRCode Read(Compiler.Compiler _)
            => code;

        CompilerObject ICTTypeCast.Cast(Compiler.Compiler compiler, IType targetType)
        {
            if (targetType == compiler.TypeSystem.Void)
            {
                if (code.IsVoid) return this;

                return new RawCode(new([
                    ..code.Instructions,
                    .. code.Types.Select(_ => new IR.VM.Pop())
                ])
                {
                    MaxStackSize = code.MaxStackSize,
                    Types = []
                });
            }

            throw new NotImplementedException();
        }

        Result<IRCode, Error> ICTCompileIRCode.CompileIRCode(Compiler.Compiler compiler)
            => Result<IRCode, Error>.Ok(code);

        IType IDynamicallyTyped.GetType(Compiler.Compiler compiler)
            => code.IsVoid ? compiler.TypeSystem.Void : code.RequireValueType();
    }
}
