using ZSharp.Compiler;

namespace ZSharp.Objects
{
    partial class GenericFunctionInstance
        : ICompileIRReference<IR.GenericFunctionInstance>
    {
        IR.GenericFunctionInstance ICompileIRReference<IR.GenericFunctionInstance>.CompileIRReference(Compiler.Compiler compiler)
        {
            var argumentResults = GenericFunction.GenericParameters
                .Select(p => GenericArguments[p])
                .Select(compiler.IR.CompileType);

            var arguments = argumentResults
                .Select(r => r.Unwrap());

            var functionIR = compiler.IR.CompileDefinition<IR.Function, IR.Module>(GenericFunction, null).Unwrap();
            var signatureIR = compiler.IR.CompileDefinition<IR.Signature, IR.Function>(Signature, functionIR).Unwrap();

            return new(functionIR)
            {
                Arguments = [.. arguments],
                Signature = signatureIR,
            };
        }
    }
}
