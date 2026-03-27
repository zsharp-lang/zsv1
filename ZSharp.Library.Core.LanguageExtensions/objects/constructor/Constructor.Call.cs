using ZSharp.Compiler;
using ZSharp.Compiler.Features.Callable;
using ZSharp.Objects;

using IR = ZSharp.IR;

namespace Core.LanguageExtensions.Objects
{
    partial class Constructor
        : ICTCallable
    {
        IResult ICTCallable.Call(Compiler compiler, Argument[] arguments)
        {
            if (Owner is null)
                return Result.Error("Constructor doesn't yet have an owner and cannot be used.");

            if (
                compiler.IR.CompileTypeAs<IR.TypeReference>(Owner, null)
                .When(out var owner)
                .Error(out var error)
            ) return Result.Error(error);

            var stream = new ZSharp.SourceCompiler.ArgumentStream(arguments);

            IR.VM.Instruction callInstruction;
            CompilerObject returnType;

            if (
                GetIR(compiler, null)
                .When(out var ir)
                .Error(out error)
            ) return Result.Error(error);

            var constructor = new IR.ConstructorReference(ir!)
            {
                OwningType = owner!
            };

            if (stream.HasArgument("this")) // Should be the first argument, not necessarily "this"
            {
                returnType = compiler.TS.Void;
                callInstruction = new IR.VM.Call(constructor);
            }
            else
            {
                stream.AddArgument(0, RawIRCode.From([], Owner));
                returnType = Owner;
                callInstruction = new IR.VM.CreateInstance(constructor);
            }

            if (
                BoundSignature.Create(compiler, (Signature as CompilerObject)!, stream)
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
