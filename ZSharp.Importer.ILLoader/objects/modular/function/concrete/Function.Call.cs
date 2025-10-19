using ZSharp.Compiler;
using ZSharp.Compiler.Features.Callable;

namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Function
        : ICTCallable
    {
        IResult ICTCallable.Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            if (
                BoundSignature.Create(compiler, signature, new ArgumentStream(arguments))
                .When(out var boundSignature)
                .Error(out var error)
            ) return Result.Error(error);

            IRCode result = new();

            foreach (var boundParameter in boundSignature!.Parameters)
            {
                if (
                    compiler.IR.CompileCode(boundParameter.ArgumentObject, null)
                    .When(out var argumentCode)
                    .Error(out error)
                    )
                    return Result.Error(error);

                result.Instructions.AddRange(argumentCode!.Instructions);
            }

            result.Instructions.Add(new IR.VM.Call(GetIR()));
            result.Types.Add(GetIR().ReturnType);

            return Result.Ok(RawIRCode.From(result.Instructions, ReturnType));
        }
    }
}
