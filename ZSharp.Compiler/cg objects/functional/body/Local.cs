using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.Objects
{
    public sealed class Local 
        : CompilerObject
        , ICTAssignable
        , ICTReadable
        , ICompileIRObject<IR.VM.Local, ICallableBody>
    {
        [Flags]
        enum BuildState
        {
            None = 0,
            Owner = 0b1,
            Initializer = 0b100,
        }

        private BuildState _state = BuildState.None;

        public IR.VM.Local? IR { get; set; }

        public required string Name { get; set; }

        public bool IsOwnerBuilt
        {
            get => _state.HasFlag(BuildState.Owner);
            set => _state = value ? _state | BuildState.Owner : _state & ~BuildState.Owner;
        }

        public bool IsInitializerBuilt
        {
            get => _state.HasFlag(BuildState.Initializer);
            set => _state = value ? _state | BuildState.Initializer : _state & ~BuildState.Initializer;
        }

        public CompilerObject? Type { get; set; }

        public CompilerObject? Initializer { get; set; }

        public IR.VM.Local CompileIRObject(Compiler.Compiler compiler, ICallableBody? owner)
        {
            if (Type is null)
                throw new();

            IR ??= new(Name, compiler.CompileIRType(Type));

            if (!IsOwnerBuilt && owner is not null)
            {
                owner.Locals.Add(IR);

                IsOwnerBuilt = true;
            }

            if (!IsInitializerBuilt && Initializer is not null)
            {
                IR.Initializer = compiler.CompileIRCode(Initializer).Instructions.ToArray();

                IsInitializerBuilt = true;
            }

            return IR;
        }

        public IRCode Read(Compiler.Compiler compiler)
            => new([new IR.VM.GetLocal(IR!)])
            {
                MaxStackSize = 1,
                Types = [Type]
            };

        public CompilerObject Assign(Compiler.Compiler compiler, CompilerObject value)
        {
            // TODO: check if read-only

            if (Type is null)
                throw new();

            var code = compiler.CompileIRCode(compiler.Cast(value, Type));

            code.Instructions.AddRange(
                [
                    new IR.VM.Dup(),
                    new IR.VM.SetLocal(IR!)
                ]
            );

            return new RawCode(code);
        }
    }
}
