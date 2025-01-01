using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class Global(string name)
        : CompilerObject
        , ICTAssignable
        , ICTReadable
        , ICompileIRObject<IR.Global, IR.Module>
    {
        [Flags]
        enum BuildState
        {
            None = 0,
            Owner = 0b1,
            Initializer = 0b10,
        }

        private BuildState _state = BuildState.None;

        public IR.Global? IR { get; set; }

        public string Name { get; } = name;

        public bool IsReadOnly { get; set; }

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

        public CompilerObject? Initializer { get; set; }

        public CompilerObject? Type { get; set; }

        public IR.Global CompileIRObject(Compiler.Compiler compiler, IR.Module? owner)
        {
            if (Type is null)
                throw new();

            IR ??= new(Name, compiler.CompileIRType(Type));

            if (!IsOwnerBuilt && owner is not null)
            {
                owner.Globals.Add(IR!);

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
            => new([new IR.VM.GetGlobal(IR!)])
            {
                MaxStackSize = 1,
                Types = [Type],
            };

        public CompilerObject Assign(Compiler.Compiler compiler, CompilerObject value)
        {
            if (IsReadOnly)
                throw new();

            if (Type is null)
                throw new();

            IR = compiler.CompileIRObject<IR.Global, IR.Module>(this, null);

            var cast = compiler.Cast(value, Type);
            var code = compiler.CompileIRCode(cast);

            code.Instructions.AddRange(
                [
                    new IR.VM.Dup(),
                    new IR.VM.SetGlobal(IR),
                ]
            );
            
            code.RequireValueType();

            return new RawCode(code);
        }
    }
}
