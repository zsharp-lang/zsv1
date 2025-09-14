namespace ZSharp.Compiler
{
    partial class Dispatcher
    {
        public static Result F(CompilerObject @object)
            => Result.Error($"Object '{@object}' does not implement F");
    }
}
