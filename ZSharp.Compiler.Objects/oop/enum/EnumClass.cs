using CommonZ;
using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class EnumClass(string? name)
        : CompilerObject
        , IType
        , ICTGetMember<MemberName>
        , ITypeAssignableToType
        , ICompileIRObject<IR.EnumClass, IR.Module>
        , ICompileIRType<IR.EnumClass>
    {
        #region Build State

        [Flags]
        enum BuildState
        {
            None = 0,
            Values = 0b1,
            Owner = 0b10,
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

        public IR.EnumClass? IR { get; set; }

        public string? Name { get; set; } = name;

        public IType? MemberType { get; set; }

        public Mapping<MemberName, EnumValue> Values { get; } = [];

        IR.EnumClass ICompileIRObject<IR.EnumClass, IR.Module>.CompileIRObject(Compiler.Compiler compiler, IR.Module? owner)
        {
            if (MemberType is null)
                throw new InvalidOperationException();

            IR ??= new(Name);

            if (IR.Type is null)
            {
                var underlyingType = compiler.IR.CompileType(MemberType).Unwrap();

                IR.Type = underlyingType;
            }

            if (!state[BuildState.Owner] && owner is not null)
            {
                state[BuildState.Owner] = true;

                owner.Types.Add(IR);
            }

            if (!state[BuildState.Values])
            {
                state[BuildState.Values] = true;

                foreach (var value in Values.Values)
                    compiler.IR.CompileDefinition<IR.EnumValue, IR.EnumClass>(value, IR);
            }

            return IR;
        }

        CompilerObjectResult ICTGetMember<string>.Member(Compiler.Compiler compiler, string member)
        {
            if (Values.TryGetValue(member, out var value))
                return CompilerObjectResult.Ok(value);

            return CompilerObjectResult.Error(
                $"Could not find member {member} in {Name ?? "<AnonymousEnum>"}"
            );
        }

        IR.EnumClass ICompileIRType<IR.EnumClass>.CompileIRType(Compiler.Compiler compiler)
        {
            return 
                compiler.IR.CompileDefinition
                <IR.EnumClass, IR.Module>
                (this, null)
                .Unwrap();
        }

        bool? ITypeAssignableToType.IsAssignableTo(Compiler.Compiler compiler, IType target)
        {
            if (compiler.TypeSystem.AreEqual(this, target))
                return true;
            if (compiler.TypeSystem.AreEqual(MemberType!, target))
                return true;

            return null;
        }
    }
}
