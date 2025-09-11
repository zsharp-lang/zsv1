namespace ZSharp.Compiler
{
    partial class Dispatcher
    {
        public static Result Member(CompilerObject @object, MemberIndex index)
            => Result.Error(
                "Object does not support member access by index."
            );

        public static Result Member(CompilerObject @object, MemberIndex index, CompilerObject value)
            => Result.Error(
                "Object does not support member assignment by index."
            );

        public static Result Member(CompilerObject @object, MemberName name)
            => Result.Error(
                "Object does not support member access by name."
            );

        public static Result Member(CompilerObject @object, MemberName name, CompilerObject value)
            => Result.Error(
                "Object does not support member assignment by name."
            );
    }
}
