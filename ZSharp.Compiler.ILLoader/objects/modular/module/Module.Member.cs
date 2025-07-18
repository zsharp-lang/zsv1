using CommonZ.Utils;

namespace ZSharp.Compiler.ILLoader.Objects
{
    partial class Module
        : ICTGetMember<MemberName>
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

        CompilerObjectResult ICTGetMember<MemberName>.Member(Compiler compiler, MemberName member)
        {
            if (!Members.TryGetValue(member, out var result))
                if ((result = LoadMember(member)) is not null)
                    Members.Add(member, result);
                else return CompilerObjectResult.Error(
                    $"Could not find member {member} in module {IL.Name}"
                );

            return CompilerObjectResult.Ok(result);
        }
    }
}
