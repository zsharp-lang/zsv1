namespace ZSharp.Compiler.ILLoader
{
    partial class ModuleBodyLoader
        : ILazilyLoadMembers
    {
        private readonly Dictionary<MemberName, List<IL.MemberInfo>> ilMembers = [];

        public void AddLazyMember(string member, IL.MemberInfo lazyLoadedMember)
        {
            if (!ilMembers.TryGetValue(member, out var list))
                ilMembers[member] = list = [];

            list.Add(lazyLoadedMember);
        }

        public void AddMember(IL.MemberInfo member)
            => AddLazyMember(member.AliasOrName(), member);

        public bool LoadMember(string member)
        {
            if (!ilMembers.TryGetValue(member, out var list))
                return false;

            foreach (var item in list)
                Container.AddMember(member, LoadMember(item));

            return true;
        }
    }
}
