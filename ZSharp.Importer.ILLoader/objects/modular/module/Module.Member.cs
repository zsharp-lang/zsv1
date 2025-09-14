using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Module
        : IAddMember
        , ICTGetMember<MemberName>
    {
        public Mapping<MemberName, CompilerObject> Members { get; } = [];

        public void AddMember(string name, CompilerObject member)
        {
            var result = OnAddResult.None;
            if (member is IOnAddTo<Module> onAdd)
                result = onAdd.OnAddTo(this);

            if (result == OnAddResult.None)
                Members.Add(name, member);
        }

        CompilerObjectResult ICTGetMember<MemberName>.Member(Compiler.Compiler compiler, MemberName member)
        {
            if (!Members.TryGetValue(member, out var result) && (result = LoadMember(member)) is null)
                return CompilerObjectResult.Error(
                    $"Could not find member {member} in module {IL.Name}"
                );

            return CompilerObjectResult.Ok(result);
        }
    }
}
