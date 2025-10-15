using ZSharp.Compiler;

namespace ZSharp.Importer.ILLoader.Objects
{
    partial class BoundMethod
        : ICTCallable
    {
        Result ICTCallable.Call(Compiler.Compiler compiler, Argument[] arguments)
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
