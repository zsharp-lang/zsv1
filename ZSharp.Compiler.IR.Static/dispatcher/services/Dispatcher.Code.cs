namespace ZSharp.Compiler.IRDispatchers.Static
{
    partial class Dispatcher
    {
        public Result<IRCode> CompileCode(CompilerObject @object, TargetPlatform? target)
        {
            var result = @base.CompileCode(@object, target);

            if (result.IsError && @object.Is<ICompileIRCode>(out var compile))
                result = compile.CompileIRCode(compiler, target);

            return result;
        }
    }
}
