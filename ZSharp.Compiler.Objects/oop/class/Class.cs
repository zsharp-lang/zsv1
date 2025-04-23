using CommonZ.Utils;
using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.Objects
{
    public sealed class Class
        : CompilerObject
        , ICompileIRObject<IR.Class, IR.Module>
        , ICompileIRType<OOPTypeReference<IR.Class>>
        , ICTCallable
        , ICTGetMember<MemberName>
        , IRTGetMember<MemberName>
    {
        #region Build State

        [Flags]
        enum BuildState
        {
            None = 0,
            Base = 0b1,
            Interfaces = 0b10,
            Body = 0b100,
            Owner = 0b1000,
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

        public IR.Class? IR { get; set; }

        public string? Name { get; set; }

        public CompilerObject? Base { get; set; }

        public CompilerObject? Constructor { get; set; }

        public Collection<CompilerObject> Interfaces { get; } = [];

        public Collection<CompilerObject> Content { get; } = [];

        public Mapping<string, CompilerObject> Members { get; } = [];

        #region Protocols

        IR.Class ICompileIRObject<IR.Class, IR.Module>.CompileIRObject(Compiler.Compiler compiler, IR.Module? owner)
        {
            IR ??= new(Name);

            if (Base is not null && !state[BuildState.Base])
            {
                state[BuildState.Base] = true;

                IR.Base = compiler.CompileIRReference<IR.OOPTypeReference<IR.Class>>(Base);
            }

            if (!state[BuildState.Interfaces])
            {
                state[BuildState.Interfaces] = true;

                foreach (var @interface in Interfaces)
                {
                    IR.InterfacesImplementations.Add(new(compiler.CompileIRReference<OOPTypeReference<IR.Interface>>(@interface)));

                    // TODO: check for interface implementation
                }
            }

            if (Content.Count > 0 && !state[BuildState.Body])
            {
                state[BuildState.Body] = true;

                foreach (var item in Content)
                    compiler.CompileIRObject(item, IR);
            }

            if (owner is not null && !state[BuildState.Owner])
            {
                state[BuildState.Owner] = true;

                owner.Types.Add(IR);
            }

            return IR;
        }

        CompilerObject ICTGetMember<string>.Member(Compiler.Compiler compiler, string member)
            => Members[member];

        CompilerObject IRTGetMember<string>.Member(Compiler.Compiler compiler, CompilerObject instance, string member)
        {
            return compiler.Map(Members[member], @object => @object is IRTBoundMember bindable ? bindable.Bind(compiler, instance) : @object);
        }

        OOPTypeReference<IR.Class> ICompileIRType<OOPTypeReference<IR.Class>>.CompileIRType(Compiler.Compiler compiler)
        {
            return new ClassReference(compiler.CompileIRObject<IR.Class, IR.Module>(this, null));
        }

        CompilerObject ICTCallable.Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            if (Constructor is null)
                throw new InvalidOperationException($"Class {Name} is not constructible");

            return compiler.Call(Constructor, arguments);
        }

        #endregion
    }
}
