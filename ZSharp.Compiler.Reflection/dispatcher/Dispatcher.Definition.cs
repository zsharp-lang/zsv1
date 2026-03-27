namespace ZSharp.Compiler
{
    partial class Dispatcher
    {
        public static bool IsSameDefinition(CompilerObject left, CompilerObject right)
            => ReferenceEquals(left, right)
            || left.Is<IIsSameDefinition>(out var l) && l.IsSameDefinition(right) // see comment in IIsSameDefinition
            || right.Is<IIsSameDefinition>(out var r) && r.IsSameDefinition(left) // same as above
            ;
    }
}
