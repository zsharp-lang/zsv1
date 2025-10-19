namespace ZSharp.Compiler
{
    partial class Dispatcher
    {
        public static IResult Do(CompilerObject @object)
            => Result.Error(
                "Object does not support do"
            );
    }
}
