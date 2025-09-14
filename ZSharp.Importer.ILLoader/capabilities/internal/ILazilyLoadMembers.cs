namespace ZSharp.Importer.ILLoader
{
    internal interface ILazilyLoadMembers
    {
        public void AddLazyMember(MemberName member, IL.MemberInfo lazyLoadedMember);
    }
}
