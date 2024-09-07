namespace ZSharp.Compiler
{
    internal interface ISetMember<T, M>
    {
        public CGObject Member(T @object, M member, CGObject value);
    }
}
