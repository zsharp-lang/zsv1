namespace ZSharp.Compiler.ILLoader.Objects
{
    partial class BoundMethod
        : ICTCallable
    {
        CompilerObjectResult ICTCallable.Call(Compiler compiler, Argument[] arguments)
        {
            if (Object is not null)
                arguments = [
                    new(Object),
                    .. arguments
                ];

            return compiler.CG.Call(Method, arguments);
        }
    }
}
