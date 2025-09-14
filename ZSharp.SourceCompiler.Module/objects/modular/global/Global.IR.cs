
using System.Diagnostics.CodeAnalysis;

namespace ZSharp.SourceCompiler.Module.Objects
{
    partial class Global
        : ICompileIRDefinitionAs<IR.Global>
        , ICompileIRDefinitionIn<IR.Module>
    {
        public IR.Global? IR { get; private set; }

        Result<IR.Global> ICompileIRDefinitionAs<IR.Global>.CompileIRDefinition(Compiler.Compiler compiler, object? target)
        {
            if (IR is null)
            {
                var typeResult = compiler.IR.CompileType(Type);

                if (
                    typeResult
                    .When(out var type)
                    .IsError
                ) return typeResult.When<IR.Global>(_ => null!);

                IR = new(Name, type!);
            }

            

            return Result<IR.Global>.Ok(IR);
        }

        void ICompileIRDefinitionIn<IR.Module>.CompileIRDefinition(Compiler.Compiler compiler, IR.Module owner, object? target)
        {
            var result = compiler.IR.CompileDefinition<IR.Global>(this, target);

            if (result.IsError) return;

            if (!state[BuildState.Owner])
            {
                owner.Globals.Add(IR!);

                state.Set(BuildState.Owner);
            }
        }
    }
}
