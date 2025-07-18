using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class EnumValue(string name)
        : CompilerObject
        , ICTGet
        , IDynamicallyTyped
        , ICompileIRCode
        , ICompileIRObject<IR.EnumValue, IR.EnumClass>
    {
        #region Build State

        [Flags]
        enum BuildState
        {
            None = 0,
            Owner = 0b1,
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

        public IR.EnumValue? IR { get; set; }

        public string Name { get; set; } = name;

        public required EnumClass Owner { get; set; }

        public required CompilerObject Value { get; set; }

        IR.EnumValue ICompileIRObject<IR.EnumValue, IR.EnumClass>.CompileIRObject(Compiler.Compiler compiler, IR.EnumClass? owner)
        {
            if (IR is null)
            {
                var code = compiler.IR.CompileCode(Value).Unwrap();

                IR = new(Name, code.Instructions);
            }

            if (!state[BuildState.Owner] && owner is not null)
            {
                state[BuildState.Owner] = true;

                owner.Values.Add(IR);
            }

            return IR;
        }

        CompilerObjectResult ICTGet.Get(Compiler.Compiler compiler)
            => compiler.CG.Get(Value);

        IType IDynamicallyTyped.GetType(Compiler.Compiler compiler)
            => compiler.TypeSystem.IsTyped(Value, out var type) 
            ? type
            : throw new();

        IRCode ICompileIRCode.CompileIRCode(Compiler.Compiler compiler)
            => compiler.CompileIRCode(Value);
    }
}
