using IRCodeResult = ZSharp.Compiler.Result<ZSharp.Compiler.IRCode, string>;

namespace ZSharp.Compiler
{
    public partial struct IR
    {
        public IRCodeResult CompileCode(CompilerObject @object)
        {
            IRCode? result = null;

            if (@object is ICompileIRCode irCode)
                result = irCode.CompileIRCode(compiler);

            else if (@object is ICTReadable ctReadable)
                result = ctReadable.Read(compiler);

            if (result is not null)
                return IRCodeResult.Ok(result);
            return IRCodeResult.Error(
                "Object cannot be compiled to IR code"
            );
        }
    }
}
