

using System.Diagnostics.CodeAnalysis;

namespace ZSharp.SourceCompiler.Module.Objects
{
    partial class Function
        : ICompileIRDefinitionAs<IR.Function>
        , ICompileIRDefinitionIn<IR.Module>
    {
        public IR.Function? IR { get; private set; }

        void ICompileIRDefinitionIn<IR.Module>.CompileIRDefinition(Compiler.Compiler compiler, IR.Module owner, object? target)
        {
            if (
                CompileIR(compiler, target)
                .When(out var ir)
                .Error(out var error)
            ) throw new InvalidOperationException(error.ToString());

            if (!state[BuildState.Owner])
            {
                state[BuildState.Owner] = true;

                owner.Functions.Add(ir!);
            }
        }

        IResult<IR.Function, Error> ICompileIRDefinitionAs<IR.Function>.CompileIRDefinition(Compiler.Compiler compiler, object? target)
            => CompileIR(compiler, target);

        private IResult<IR.Function, Error> CompileIR(Compiler.Compiler compiler, object? target)
        {
            if (ReturnType is null)
                return Result<IR.Function>.Error(
                    $"Function {Name} does not have a return type"
                );
            if (
                compiler.IR.CompileType(ReturnType)
                .When(out var returnType)
                .Error(out var error)
            ) return Result<IR.Function>.Error(error);

            IR ??= new(returnType!)
            {
                Name = Name
            };

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
