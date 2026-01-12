using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Class
        : IAddMember
        , ICTGetMember<MemberName>
        , IRTGetMember<MemberName>
    {
        public Mapping<MemberName, CompilerObject> Members { get; } = [];

        CompilerObject IAddMember.AddMember(string name, CompilerObject member)
        {
            var result = OnAddResult.None;
            if (member is IOnAddTo<Class> onAdd)
                result = onAdd.OnAddTo(this);

            if (result == OnAddResult.None)
                Members.Add(name, member);

            return member;
        }

        IResult ICTGetMember<MemberName>.Member(Compiler.Compiler compiler, MemberName name)
        {
            if (!Members.TryGetValue(name, out var member) && !LazyLoader.GetLazyMember(name, out member))
                return Result.Error(
                    $"Could not find member {name} in type {IL.Name}"
                );

            return Result.Ok(member);
        }

        IResult IRTGetMember<string>.Member(Compiler.Compiler compiler, CompilerObject @object, MemberName name)
        {
            if (
                compiler.CG.Member(this, name)
                .When(out var member)
                .Error(out var error)
            ) return Result.Error(error);

            if (member!.Is<IBindable>(out var bindable))
                return bindable.Bind(compiler, @object);

            return Result.Ok(member);
        }
    }
}
