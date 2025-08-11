using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class ArrayTypeObject(IR.Class ir)
        : CompilerObject
        , IType
        , ICompileIRObject
        , IGenericInstantiable
    {
        public IR.Class IR { get; } = ir;

        CompilerObjectResult IGenericInstantiable.Instantiate(Compiler.Compiler compiler, Argument[] arguments)
        {
            if (arguments.Length != 1)
                return CompilerObjectResult.Error(
                    $"Expected exactly 1 generic argument, but got {arguments.Length}"
                );

            var elementTypeArgument = arguments[0];
            if (elementTypeArgument.Name is not null)
                return CompilerObjectResult.Error(
                    "Array[] type argument must be positional"
                );
            if (elementTypeArgument.Object is not IType elementType)
                return CompilerObjectResult.Error(
                    "Array[] type argument must be a type"
                );

            return CompilerObjectResult.Ok(
                Instantiate(elementType)
            );
        }

        public ArrayType Instantiate(IType elementType)
            => new(elementType);

        IR.IRDefinition ICompileIRObject.CompileIRObject(Compiler.Compiler compiler)
            => IR;
    }
}
