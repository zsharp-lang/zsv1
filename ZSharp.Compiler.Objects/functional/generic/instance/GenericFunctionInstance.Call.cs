using ZSharp.Compiler;

namespace ZSharp.Objects
{
    partial class GenericFunctionInstance
        : ICTCallable
    {
        CompilerObjectResult ICTCallable.Call(Compiler.Compiler compiler, Argument_NEW<CompilerObject>[] arguments)
        {
            if (ReturnType is null)
                return CompilerObjectResult.Error(
                    "Return type is not defined"
                );

            var args = Signature.MatchArguments(
                compiler, 
                arguments
                .Select(arg => new Argument(arg.Name, arg.Value))
                .ToArray()
            );

            IRCode code = new();

            List<CompilerObject> @params = [];

            @params.AddRange(Signature.Args);

            if (Signature.VarArgs is not null)
                @params.Add(Signature.VarArgs);

            @params.AddRange(Signature.KwArgs);

            if (Signature.VarKwArgs is not null)
                @params.Add(Signature.VarKwArgs);

            foreach (var param in @params)
                code.Append(compiler.CompileIRCode(args[param]));

            code.Append(new([
                new IR.VM.Call(compiler.CompileIRReference<IR.ConstructedFunction>(this))
            ]));

            code.Types.Clear();
            if (ReturnType != compiler.TypeSystem.Void)
                code.Types.Add(ReturnType);

            return CompilerObjectResult.Ok(new RawCode(code));
        }
    }
}
