namespace ZSharp.Compiler
{
    partial class IR
    {
        public Result<IRCode> CompileCode(CompilerObject @object, TargetPlatform? target)
        {
            if (@object.Is<ICompileIRCode>(out var compile))
                return compile.CompileIRCode(this, target);

            return Result<IRCode>.Error(
                $"Object of type { @object.GetType().Name } does not implement ICompileIRCode"
            );
        }
    }
}
