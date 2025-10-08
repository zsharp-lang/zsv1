using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class ValueType(string? name)
        : CompilerObject
        , IType
        , IIRReferenceCompiler<IR.ValueTypeReference>
        , ICompileIRType<IR.TypeReference<IR.ValueType>>
    {
        #region Build State

        [Flags]
        enum BuildState
        {
            None = 0,
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

        public IR.ValueType? IR { get; set; }

        public string? Name { get; set; } = name;

        private IR.ValueType CompileIR(Compiler.Compiler compiler)
        {
            IR ??= new(Name);

            return IR;
        }

        private IR.ValueTypeReference CompileIRReference(Compiler.Compiler compiler)
        {
            return new(CompileIR(compiler));
        }

        IR.TypeReference<IR.ValueType> ICompileIRType<IR.TypeReference<IR.ValueType>>.CompileIRType(Compiler.Compiler compiler)
            => CompileIRReference(compiler);

        IR.ValueTypeReference IIRReferenceCompiler<IR.ValueTypeReference>.CompileIRReference(Compiler.Compiler compiler)
            => CompileIRReference(compiler);
    }
}
