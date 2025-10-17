using CommonZ.Utils;
using ZSharp.Compiler.Features.Callable;

namespace ZSharp.Importer.ILLoader.Objects
{
    internal sealed partial class Signature
        : CompilerObject
        , ISignature
    {
        internal readonly Collection<IParameter> parameters = [];

        IEnumerable<CompilerObject> ISignature.Parameters(Compiler.Compiler compiler)
            => parameters.Cast<CompilerObject>();
    }
}
