using CommonZ.Utils;

namespace ZSharp.Compiler.ILLoader.Objects
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

        CompilerObjectResult ICTGetMember<MemberName>.Member(Compiler compiler, MemberName member)
        {
            if (Members.TryGetValue(member, out var result))
                return CompilerObjectResult.Ok(result);

            return CompilerObjectResult.Error(
                $"Can't find member {member} in namespace {Name}"
            );
        }
    }
}
