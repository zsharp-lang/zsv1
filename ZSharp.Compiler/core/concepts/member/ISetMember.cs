namespace ZSharp.Compiler
{
    internal interface ISetMember<T, M>
    {
        public CompilerObject Member(T @object, M member, CompilerObject value);
    }
}
