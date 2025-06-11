using IRCodeResult = ZSharp.Compiler.Result<ZSharp.Compiler.IRCode, string>;

namespace ZSharp.Compiler
{
    public partial struct IR
    {
        public IRCodeResult CompileCode(CompilerObject @object)
        {
            var result = IRCodeResult.Error(
                "Object does not support compiling to IR code"
            );

            if (@object is ICTCompileIRCode ctIRCode)
                result = ctIRCode.CompileIRCode(compiler);

            if (result.IsOk) return result;

            return result;
        }
    }
}
