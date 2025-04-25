using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class Local 
        : CompilerObject
        , ICTAssignable
        , ICTReadable
        , ICompileIRObject<IR.VM.Local, IR.ICallableBody>
    {
        [Flags]
        enum BuildState
        {
            None = 0,
            Owner = 0b1,
            Initializer = 0b100,
        }

        private readonly ObjectBuildState<BuildState> state = new();

        public IR.VM.Local? IR { get; set; }

        public required string Name { get; set; }

        public bool IsReadOnly { get; set; }

        public IType? Type { get; set; }

        public CompilerObject? Initializer { get; set; }

        public IR.VM.Local CompileIRObject(Compiler.Compiler compiler, IR.ICallableBody? owner)
        {
            if (Type is null)
                throw new PartiallyCompiledObjectException(
                    this, $"Local variable {Name} does not have a type"
                );

            IR ??= new(Name, compiler.CompileIRType(Type));

            if (owner is not null && !state[BuildState.Owner])
            {
                state[BuildState.Owner] = true;

                owner.Locals.Add(IR);
            }

            if (Initializer is not null && !state[BuildState.Initializer])
            {
                state[BuildState.Initializer] = true;

                IR.Initializer = [.. compiler.CompileIRCode(Initializer).Instructions];
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
            if (IsReadOnly)
                throw new();

            if (Type is null)
                if (compiler.TypeSystem.IsTyped(value, out var valueType))
                    Type = valueType;
                else throw new PartiallyCompiledObjectException(this, $"Local variable {Name} does not have a type");

            IR ??= CompileIRObject(compiler, null);

            if (!compiler.TypeSystem.ImplicitCast(value, Type).Ok(out var cast))
                throw new Compiler.InvalidCastException(value, Type);

            var code = compiler.CompileIRCode(cast);

            code.Instructions.AddRange(
                [
                    new IR.VM.Dup(),
                    new IR.VM.SetLocal(IR)
                ]
            );

            return new RawCode(code);
        }
    }
}
