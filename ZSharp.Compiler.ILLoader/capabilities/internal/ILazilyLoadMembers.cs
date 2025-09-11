namespace ZSharp.Compiler.ILLoader
{
    internal interface ILazilyLoadMembers
    {
        public void AddLazyMember(MemberName member, IL.MemberInfo lazyLoadedMember);
    }
}
