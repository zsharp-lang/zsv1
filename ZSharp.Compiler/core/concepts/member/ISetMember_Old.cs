namespace ZSharp.Compiler
{
    internal interface ISetMember_Old<T, M>
    {
        public CompilerObject Member(T @object, M member, CompilerObject value);
    }
}
