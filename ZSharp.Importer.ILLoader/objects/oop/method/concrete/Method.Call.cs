
using ZSharp.Compiler;

namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Method
        : ICTCallable
    {
        CompilerObjectResult ICTCallable.Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            IRCode result = new();

            foreach (var argument in arguments)
            {
                if (
                    compiler.IR.CompileCode(argument.Object, null)
                    .When(out var argumentCode)
                    .Error(out var error)
                    )
                    return CompilerObjectResult.Error(error);

                result.Instructions.AddRange(argumentCode!.Instructions);
            }

            result.Instructions.Add(new IR.VM.Call(GetIR()));
            result.Types.Add(GetIR().ReturnType);

            return CompilerObjectResult.Ok(new RawIRCode(result));
        }
    }
}
