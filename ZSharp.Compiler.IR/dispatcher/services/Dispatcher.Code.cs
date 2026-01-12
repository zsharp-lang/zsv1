namespace ZSharp.Compiler
{
    partial class Dispatcher
    {
        public IResult<IRCode, Error> CompileCode(CompilerObject @object, TargetPlatform? target)
            => Result<IRCode>.Error(
                $"Object {@object} does not support compile IR code"
            );
    }
}
