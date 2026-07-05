namespace ZSharp.SourceCompiler.Module.Objects
{
    partial class Module
        : ICTGetMember<MemberName>
    {
        private readonly Dictionary<string, CompilerObject> members = [];

        public IResult AddMember(string name, CompilerObject member)
        {
            if (members.ContainsKey(name))
                return Result.Error($"Member '{name}' is already defined in module '{Name}'.");
            else
                members[name] = member;

            return Result.Ok(member);
        }

        IResult ICTGetMember<string>.Member(ZSharp.Compiler.Compiler compiler, string member)
        {
            if (members.TryGetValue(member, out var obj))
                return Result<CompilerObject>.Ok(obj);

            return Result<CompilerObject>.Error($"Member '{member}' not found in module '{Name}'.");
        }
    }
}
