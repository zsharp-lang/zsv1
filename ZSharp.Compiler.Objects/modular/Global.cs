using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class Global(string name)
        : CompilerObject
        , ICTAssignable
        , ICTReadable
        , ICompileIRObject<IR.Global, IR.Module>
    {
        #region Build State

        [Flags]
        enum BuildState
        {
            None = 0,
            Owner = 0b1,
            Initializer = 0b10,
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

        public IR.Global? IR { get; set; }

        public string Name { get; } = name;

        public bool IsReadOnly { get; set; }

        public CompilerObject? Initializer { get; set; }

        public CompilerObject? Type { get; set; }

        #region Protocols

        public IR.Global CompileIRObject(Compiler.Compiler compiler, IR.Module? owner)
        {
            if (Type is null)
                throw new PartiallyCompiledObjectException(
                    this,
                    Errors.UndefinedGlobalType(Name)
                );

            IR ??= new(Name, compiler.CompileIRType(Type));

            if (owner is not null && !state[BuildState.Owner])
            {
                state[BuildState.Owner] = true;

                owner.Globals.Add(IR!);
            }

            if (Initializer is not null && !state[BuildState.Initializer])
            {
                state[BuildState.Initializer] = true;

                if (!compiler.TypeSystem.ImplicitCast(Initializer, Type).Ok(out var initializer))
                    throw new Compiler.InvalidCastException(Initializer, Type);

                IR.Initializer = [.. compiler.CompileIRCode(initializer).Instructions];
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

            if (!compiler.TypeSystem.ImplicitCast(value, Type).Ok(out var cast))
                throw new Compiler.InvalidCastException(value, Type);

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

        #endregion
    }
}
