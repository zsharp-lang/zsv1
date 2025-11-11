using ZSharp.Compiler;
using ZSharp.Compiler.Features.Callable;

namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Constructor
        : ICTCallable
    {
        IResult ICTCallable.Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            if (Owner is null)
                return Result.Error("Constructor doesn't yet have an owner and cannot be used.");

            if (
                compiler.IR.CompileReference<IR.TypeReference>(Owner, null)
                .When(out var owner)
                .Error(out var error)
            ) return Result.Error(error);

            var stream = new ArgumentStream(arguments);

            IR.VM.Instruction callInstruction;
            CompilerObject returnType;

            var constructor = new IR.ConstructorReference(GetIR(null))
            {
                OwningType = owner!
            };

            if (stream.HasArgument(thisParameter.Name))
            {
                returnType = compiler.TS.Void;
                callInstruction = new IR.VM.Call(constructor);
            } else
            {
                stream.args.Insert(0, RawIRCode.From([], Owner));
                returnType = Owner;
                callInstruction = new IR.VM.CreateInstance(constructor);
            }

            if (
                BoundSignature.Create(compiler, signature, stream)
                .When(out var boundSignature)
                .Error(out error)
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

            result.Instructions.Add(callInstruction);

            return Result.Ok(RawIRCode.From(result.Instructions, returnType));
        }
    }
}
