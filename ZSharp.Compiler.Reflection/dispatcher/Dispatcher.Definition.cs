namespace ZSharp.Compiler
{
    partial class Dispatcher
    {
        public static bool IsSameDefinition(CompilerObject left, CompilerObject right)
            => ReferenceEquals(left, right);
    }
}
