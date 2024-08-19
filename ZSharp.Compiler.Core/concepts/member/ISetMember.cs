namespace ZSharp.CTRuntime
{
    internal interface ISetMember<T, M>
    {
        public Code Member(T @object, M member, Code value);
    }
}
