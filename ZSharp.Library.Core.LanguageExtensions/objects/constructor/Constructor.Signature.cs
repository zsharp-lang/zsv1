using ZSharp.Compiler.Features.Callable;
using ZSharp.SourceCompiler;

namespace Core.LanguageExtensions.Objects
{
    partial class Constructor
    {
        public ISignature Signature => (this as CompilerObject).As<IConstructor>()!.Signature;
    }
}
