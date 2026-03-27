namespace ZSharp.SourceCompiler.Objects
{
    partial class Constructor
        : ICompileIRDefinitionAs<IR.Function>
    {
        public IR.Function? IR { get; private set; }

        IResult<IR.Function, Error> ICompileIRDefinitionAs<IR.Function>.CompileIRDefinition(Compiler.Compiler compiler, object? target)
            => CompileIR(compiler, target);

        private IResult<IR.Function, Error> CompileIR(Compiler.Compiler compiler, object? target)
        {
            if (
                compiler.IR.CompileType(compiler.TS.Void, target)
                .When(out var returnType)
                .Error(out var error)
            ) return Result<IR.Function>.Error(error);

            IR ??= new(returnType!)
            {
                Name = Name
            };

            if (!state[BuildState.Signature])
            {
                state[BuildState.Signature] = true;

                foreach (var parameter in Signature.Parameters(compiler))
                    if (
                        compiler.IR.CompileDefinition<IR.Parameter>(parameter, target)
                        .When(out var irParameter)
                        .Error(out error)
                    ) return Result<IR.Function>.Error(error);
                    else
                        IR.Signature.Args.Parameters.Add(irParameter!);
            }

            if (!state[BuildState.Body])
            {
                state[BuildState.Body] = true;

                if (Body is not null)
                    if (
                        compiler.IR.CompileCode(Body, target)
                        .When(out var body)
                        .Error(out error)
                    ) return Result<IR.Function>.Error(error);
                    else
                    {
                        IR.Body.Instructions.AddRange(body!.Instructions);
                        IR.Body.StackSize = body.MaxStackSize;
                    }
            }

            return Result<IR.Function>.Ok(IR);
        }
    }
}
