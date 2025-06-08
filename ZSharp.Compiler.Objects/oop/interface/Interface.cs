using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class Interface(string? name)
        : CompilerObject
        , IAbstraction
        , ICompileIRObject<IR.Interface, IR.Module>
        , ICompileIRReference<IR.OOPTypeReference<IR.Interface>>
        , ICompileIRType<IR.OOPTypeReference<IR.Interface>>
        , ICTGetMember_Old<MemberName>
        , IRTGetMember_Old<MemberName>
        , IType
    {
        #region Build State

        [Flags]
        enum BuildState
        {
            None = 0b0,
            Owner = 0b1,
            Bases = 0b10,
            Body = 0b100,
        }

        private readonly ObjectBuildState<BuildState> state = new();

        public bool IsDefined
        {
            init
            {
                if (value)
                    foreach (var item in Enum.GetValues<BuildState>())
                        state[item] = true;
            }
        }

        #endregion

        public IR.Interface? IR { get; set; }

        public string Name { get; set; } = name ?? string.Empty;

        public Collection<CompilerObject> Bases { get; } = [];

        public Collection<CompilerObject> Content { get; } = [];

        public Mapping<MemberName, CompilerObject> Members = [];

        #region Protocols

        Collection<CompilerObject> IAbstraction.Specifications => Content;

        CompilerObject ICTGetMember_Old<string>.Member(Compiler.Compiler compiler, string member)
            => Members[member];

        CompilerObject IRTGetMember_Old<string>.Member(Compiler.Compiler compiler, CompilerObject value, string member)
            => compiler.Map(Members[member], @object => @object is IRTBoundMember bindable ? bindable.Bind(compiler, value) : @object);

        IR.Interface ICompileIRObject<IR.Interface, IR.Module>.CompileIRObject(Compiler.Compiler compiler, IR.Module? owner)
        {
            IR ??= new(Name);

            if (owner is not null && !state[BuildState.Owner])
            {
                state[BuildState.Owner] = true;

                owner.Types.Add(IR);
            }

            if (!state[BuildState.Bases])
            {
                state[BuildState.Bases] = true;

                foreach (var @base in Bases)
                    IR.Bases.Add(compiler.CompileIRReference<IR.OOPTypeReference<IR.Interface>>(@base));
            }

            if (!state[BuildState.Body])
            {
                state[BuildState.Body] = true;

                foreach (var item in Content)
                    compiler.CompileIRObject(item, IR);
            }

            return IR;
        }

        IR.OOPTypeReference<IR.Interface> ICompileIRReference<IR.OOPTypeReference<IR.Interface>>.CompileIRReference(Compiler.Compiler compiler)
            => new IR.InterfaceReference(
                compiler.CompileIRObject<IR.Interface, IR.Module>(this, null)
            );

        IR.OOPTypeReference<IR.Interface> ICompileIRType<IR.OOPTypeReference<IR.Interface>>.CompileIRType(Compiler.Compiler compiler)
            => new IR.InterfaceReference(
                compiler.CompileIRObject<IR.Interface, IR.Module>(this, null)
            );

        #endregion
    }
}
