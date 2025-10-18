using System.Reflection;

namespace ZSharp.Importer.ILLoader.Objects
{
    partial class LazyMemberLoader
        : ILazyMemberLoader
    {
        void ILazyMemberLoader.AddLazyMember(string name, MemberInfo member)
        {
            if (!lazyMembers.TryGetValue(name, out var members))
                lazyMembers[name] = members = [];

            members.Add(member);
        }

        CompilerObject? ILazyMemberLoader.GetLazyMember(string name)
        {
            if (!lazyMembers.TryGetValue(name, out var members))
                return null;

            CompilerObject? result = null;

            foreach (var member in members)
                result = Container.AddMember(name, Loader.LoadMember(member));

            return result;
        }
    }
}
