namespace ZSharp.SourceCompiler.Module.Objects
{
    partial class Global
        : ICompileIRDefinitionAs<IR.Global>
        , ICompileIRDefinitionIn<IR.Module>
    {
        public IR.Global? IR { get; private set; }

        IResult<IR.Global, Error> ICompileIRDefinitionAs<IR.Global>.CompileIRDefinition(ZSharp.Compiler.Compiler compiler, object? target)
        {
            if (IR is null)
            {
                var typeResult = compiler.IR.CompileType(Type, target);

                if (
                    typeResult
                    .When(out var type)
                    .IsError
                ) return typeResult.When(_ => (null as IR.Global)!);

                IR = new(Name, type!);
            }

            

            return Result<IR.Global>.Ok(IR);
        }

        void ICompileIRDefinitionIn<IR.Module>.CompileIRDefinition(ZSharp.Compiler.Compiler compiler, IR.Module owner, object? target)
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
