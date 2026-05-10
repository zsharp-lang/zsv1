using ZSharp.Compiler;
using ZSharp.Compiler.Features.OOP;

namespace Core.Runtime.Objects
{
    partial class GenericClass
        : ICTGetMember<MemberName>
        , IRTGetMember<MemberName>
    {
        public Dictionary<MemberName, CompilerObject> MembersByName { get; } = [];

        public List<CompilerObject> MembersByOrder { get; } = [];

        IResult ICTGetMember<MemberName>.Member(Compiler compiler, MemberName name)
        {
            if (!MembersByName.TryGetValue(name, out var member))
                return Result.Error(
                    $"Could not find member {name} in class {Name}"
                );

            return Result.Ok(member);
        }

        IResult IRTGetMember<MemberName>.Member(Compiler compiler, CompilerObject @object, MemberName name)
        {
            if (
                compiler.CG.Member(this, name)
                .When(out var member)
                .Error(out var error)
            ) return Result.Error(error);

            if (member!.Is<IBindable>(out var bindable))
                return bindable.Bind(compiler, @object);
            else if (member.Is<ICOProxy>(out var proxy))
                return proxy.Apply(obj =>
                {
                    if (obj.Is<IBindable>(out var bindableProxy))
                        return bindableProxy.Bind(compiler, @object);
                    else
                        return Result.Ok(obj);
                });

            return Result.Ok(member);
        }
    }
}
