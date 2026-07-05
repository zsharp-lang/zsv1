namespace ZSharp.SourceCompiler.Module.Objects
{
    partial class Module
        : ICompileIRDefinitionAs<IR.Module>
    {
        public IR.Module? IR { get; private set; }

        IResult<IR.Module, Error> ICompileIRDefinitionAs<IR.Module>.CompileIRDefinition(ZSharp.Compiler.Compiler compiler, object? target)
        {
            IR ??= new(Name);

            if (!state[BuildState.Content])
            {
                state[BuildState.Content] = true;

                foreach (var item in Content)
                    compiler.IR.CompileDefinition(item, IR, target);
            }

            if (!state[BuildState.EntryPoint])
            {
                if (EntryPoint is not null)
                    if (
                        compiler.IR.CompileDefinition<IR.Function>(EntryPoint, target)
                        .When(out var entryPoint)
                        .Error(out var error)
                    ) return Result<IR.Module>.Error(error);
                    else IR.EntryPoint = entryPoint;

                state[BuildState.EntryPoint] = true;
            }

            if (!state[BuildState.Initializer])
            {
                if (Initializer is not null)
                    if (
                        compiler.IR.CompileDefinition<IR.Function>(Initializer, target)
                        .When(out var initializer)
                        .Error(out var error)
                    ) return Result<IR.Module>.Error(error);
                    else IR.Initializer = initializer;

                state[BuildState.Initializer] = true;
            }

            return Result<IR.Module>.Ok(IR);
        }
    }
}
