using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Namespace
        : ICTGetMember<MemberName>
    {
        public Mapping<MemberName, CompilerObject> Members { get; } = [];

        public void AddMember(string name, CompilerObject member)
        {
            var result = OnAddResult.None;
            if (member is IOnAddTo<Namespace> onAdd)
                result = onAdd.OnAddTo(this);

            if (result == OnAddResult.None)
                Members.Add(name, member);
        }

        IResult ICTGetMember<MemberName>.Member(Compiler.Compiler compiler, MemberName member)
        {
            if (!Members.TryGetValue(member, out var result))
                if ((result = GetLazyMember(member)) is not null)
                    AddMember(member, result);
                else return Result.Error(
                    $"Could not find member {member} in namespace {FullName}"
                );

            return Result.Ok(result);
        }
    }
}
