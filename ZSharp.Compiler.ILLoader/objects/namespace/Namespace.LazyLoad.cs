namespace ZSharp.Compiler.ILLoader.Objects
{
    partial class Namespace
        : ILazilyLoadMembers
    {
        private readonly Dictionary<MemberName, IL.MemberInfo> lazyLoadedMembers = [];

        public ILLoader Loader { get; } = loader;

        public void AddLazyMember(MemberName member, IL.MemberInfo lazyLoadedMember)
            => lazyLoadedMembers.Add(member, lazyLoadedMember);

        public CompilerObject? LoadMember(string name)
        {
            if (!lazyLoadedMembers.TryGetValue(name, out var member))
                return null;

            return Loader.LoadMember(member);
        }
    }
}
