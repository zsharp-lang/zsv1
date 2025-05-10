using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class Class
        : CompilerObject
        , ICompileIRObject<IR.Class, IR.Module>
        , ICompileIRReference<IR.ClassReference>
        , ICompileIRType<IR.OOPTypeReference<IR.Class>>
        , ICTCallable
        , ICTGetMember<MemberName>
        , IRTGetMember<MemberName>
        , IImplementsAbstraction
        , IType
        , ITypeAssignableToType
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

        public IType? Base { get; set; }

        public CompilerObject? Constructor { get; set; }

        public Collection<IType> Interfaces { get; } = [];

        public Collection<Implementation> InterfaceImplementations { get; } = [];

        public Collection<CompilerObject> Content { get; } = [];

        public Mapping<string, CompilerObject> Members { get; } = [];

        #region Protocols

        IR.Class ICompileIRObject<IR.Class, IR.Module>.CompileIRObject(Compiler.Compiler compiler, IR.Module? owner)
        {
            IR ??= new(Name);

            if (owner is not null && !state[BuildState.Owner])
            {
                state[BuildState.Owner] = true;

                owner.Types.Add(IR);
            }

            if (Base is not null && !state[BuildState.Base])
            {
                state[BuildState.Base] = true;

                IR.Base = compiler.CompileIRReference<IR.OOPTypeReference<IR.Class>>(Base);
            }

            if (Content.Count > 0 && !state[BuildState.Body])
            {
                state[BuildState.Body] = true;

                foreach (var item in Content)
                    compiler.CompileIRObject(item, IR);
            }

            if (InterfaceImplementations.Count > 0 && !state[BuildState.Interfaces])
            {
                state[BuildState.Interfaces] = true;

                foreach (var @interfaceImplementation in InterfaceImplementations)
                {
                    IR.InterfaceImplementation implementation = new(
                        compiler.CompileIRReference<IR.OOPTypeReference<IR.Interface>>(interfaceImplementation.Abstract)
                    );

                    IR.InterfacesImplementations.Add(implementation);

                    foreach (var (@abstract, concrete) in interfaceImplementation.Mapping)
                    {
                        var abstractMethod = compiler.CompileIRReference<IR.MethodReference>(@abstract);
                        var concreteMethod = compiler.CompileIRObject<IR.Method, IR.Class>(concrete, IR);
                        implementation.Implementations.Add(abstractMethod, concreteMethod);
                    }
                }
            }

            return IR;
        }

        CompilerObject ICTGetMember<string>.Member(Compiler.Compiler compiler, string member)
            => Members[member];

        CompilerObject IRTGetMember<string>.Member(Compiler.Compiler compiler, CompilerObject instance, string member)
        {
            return compiler.Map(Members[member], @object => @object is IRTBoundMember bindable ? bindable.Bind(compiler, instance) : @object);
        }

        IR.OOPTypeReference<IR.Class> ICompileIRType<IR.OOPTypeReference<IR.Class>>.CompileIRType(Compiler.Compiler compiler)
        {
            return new IR.ClassReference(compiler.CompileIRObject<IR.Class, IR.Module>(this, null));
        }

        IR.ClassReference ICompileIRReference<IR.ClassReference>.CompileIRReference(Compiler.Compiler compiler)
        {
            return new IR.ClassReference(compiler.CompileIRObject<IR.Class, IR.Module>(this, null));
        }


        CompilerObject ICTCallable.Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            if (Constructor is null)
                throw new InvalidOperationException($"Class {Name} is not constructible");

            return compiler.Call(Constructor, arguments);
        }

        bool? ITypeAssignableToType.IsAssignableTo(Compiler.Compiler compiler, IType target)
        {
            if (Base is not null && compiler.TypeSystem.IsAssignableTo(Base, target))
                return true;

            foreach (var @interface in Interfaces)
            {
                if (compiler.TypeSystem.IsAssignableTo(@interface, target))
                    return true;
            }

            return null;
        }

        #endregion
    }
}
