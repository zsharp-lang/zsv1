using ZSharp.Compiler;

namespace Core.LanguageExtensions.Objects
{
    partial class Method
        : ICOProxy
    {
        public required CompilerObject UnderlyingFunction { private get; init; }

        IResult ICOProxy.Apply(Func<CompilerObject, IResult> fn)
            => fn(UnderlyingFunction);
    }
}
