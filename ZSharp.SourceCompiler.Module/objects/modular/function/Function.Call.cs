using ZSharp.Objects;

namespace ZSharp.SourceCompiler.Module.Objects
{
    partial class Function
        : ICTCallable
    {
        Result<CompilerObject> ICTCallable.Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            IRCode result = new();

            foreach (var argument in arguments)
            {
                if (
                    compiler.IR.CompileCode(argument.Object, null)
                    .When(out var argumentCode)
                    .Error(out var error)
                    )
                    return Result.Error(error);

                result.Instructions.AddRange(argumentCode!.Instructions);
            }

            if (IR is null)
                if (CompileIR(compiler, null).When(out var ir).Error(out var error))
                    return Result.Error(error);
                else IR = ir!;

            result.Instructions.Add(new IR.VM.Call(IR));
            result.Types.Add(IR.ReturnType);

            return Result.Ok(new RawIRCode(result));
        }
    }
}
