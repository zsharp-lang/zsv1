namespace ZSharp.Compiler
{
    partial class Dispatcher
    {
        public static IResult Get(CompilerObject @object)
            => Result.Error(
                "Object does not support get"
            );

        public static IResult Set(CompilerObject @object, CompilerObject value)
            => Result.Error(
                "Object does not support set"
            );
    }
}
