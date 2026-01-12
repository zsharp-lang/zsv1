using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Module
        : IAddMember
        , ICTGetMember<MemberName>
    {
        public Mapping<MemberName, CompilerObject> Members { get; } = [];

        CompilerObject IAddMember.AddMember(string name, CompilerObject member)
        {
            var result = OnAddResult.None;
            if (member is IOnAddTo<Module> onAdd)
                result = onAdd.OnAddTo(this);

            if (result == OnAddResult.None)
                Members.Add(name, member);

            return member;
        }

        IResult ICTGetMember<MemberName>.Member(Compiler.Compiler compiler, MemberName name)
        {
            if (!Members.TryGetValue(name, out var member) && !LazyLoader.GetLazyMember(name, out member))
                return Result.Error(
                    $"Could not find member {name} in module {IL.Name}"
                );

            return Result.Ok(member);
        }
    }
}
