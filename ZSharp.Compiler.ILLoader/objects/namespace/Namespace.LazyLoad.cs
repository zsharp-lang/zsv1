namespace ZSharp.Compiler.ILLoader.Objects
{
    partial class Namespace
    {
        private readonly Dictionary<MemberName, Func<CompilerObject>> lazyLoadedMembers = [];

        public void AddLazyMember(MemberName member, Func<CompilerObject> lazyLoadedMember)
            => lazyLoadedMembers.Add(member, lazyLoadedMember);
    }
}
