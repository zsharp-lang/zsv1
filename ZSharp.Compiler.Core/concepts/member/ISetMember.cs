namespace ZSharp.Compiler
{
    internal interface ISetMember<T, M>
    {
        public CTObject Member(T @object, M member, CTObject value);
    }
}
