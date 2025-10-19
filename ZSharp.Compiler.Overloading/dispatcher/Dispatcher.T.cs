namespace ZSharp.Compiler
{
    partial class Dispatcher
    {
        public static IResult F(CompilerObject @object)
            => Result.Error($"Object '{@object}' does not implement F");
    }
}
