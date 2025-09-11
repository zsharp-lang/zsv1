namespace ZSharp.Compiler
{
    partial class Dispatcher
    {
        public static Result RuntimeDescriptor(CompilerObject @object)
            => Result.Error(
                "Object does not support runtime descriptor protocol"
            );
    }
}
