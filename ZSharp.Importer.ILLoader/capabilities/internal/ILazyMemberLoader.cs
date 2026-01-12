using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Importer.ILLoader
{
    internal interface ILazyMemberLoader
    {
        public void AddLazyMember(MemberName name, IL.MemberInfo member);

        public CompilerObject? GetLazyMember(MemberName name);

        public bool GetLazyMember(MemberName name, [NotNullWhen(true)] out CompilerObject? member)
            => (member = GetLazyMember(name)) is not null;
    }
}
