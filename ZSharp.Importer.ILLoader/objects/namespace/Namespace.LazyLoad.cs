namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Namespace
        : ILazyMemberLoader
    {
        private readonly Dictionary<MemberName, IL.MemberInfo> lazyLoadedMembers = [];

        public ILLoader Loader { get; } = loader;

        public void AddLazyMember(MemberName member, IL.MemberInfo lazyLoadedMember)
            => lazyLoadedMembers.Add(member, lazyLoadedMember);

        public CompilerObject? GetLazyMember(string name)
        {
            if (!lazyLoadedMembers.TryGetValue(name, out var member))
                return null;

            return Loader.LoadMember(member);
        }
    }
}
