using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class Class
        : CompilerObject
        , ICompileIRObject<IR.Class, IR.Module>
    {
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

        public IR.Class? IR { get; private set; }

        public string? Name { get; set; }

        public CompilerObject? Base { get; set; }

        public Collection<CompilerObject> Interfaces { get; } = [];

        public Collection<CompilerObject> Content { get; } = [];

        IR.Class ICompileIRObject<IR.Class, IR.Module>.CompileIRObject(Compiler.Compiler compiler, IR.Module? owner)
        {
            IR ??= new(Name);

            if (Base is not null && !state[BuildState.Base])
            {
                state[BuildState.Base] = true;

                IR.Base = compiler.CompileIRReference<IR.OOPTypeReference<IR.Class>>(Base);
            }

            if (Interfaces.Count > 0 && !state[BuildState.Interfaces])
            {
                throw new NotImplementedException($"Interfaces are not supported yet.");
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
    }
}
