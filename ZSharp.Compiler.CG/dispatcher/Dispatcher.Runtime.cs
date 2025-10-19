namespace ZSharp.Compiler
{
    partial class Dispatcher
    {
        public static IResult RuntimeDescriptor(CompilerObject @object)
            => Result.Error(
                "Object does not support runtime descriptor protocol"
            );
    }
}
