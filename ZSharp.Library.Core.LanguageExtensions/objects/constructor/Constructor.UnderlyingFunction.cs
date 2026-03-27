using System.Diagnostics.CodeAnalysis;

namespace Core.LanguageExtensions.Objects
{
    partial class Constructor
    {
        public required CompilerObject UnderlyingObject { private get; init; }

        public ZSharp.SourceCompiler.IConstructor UnderlyingConstructor 
            => UnderlyingObject.As<ZSharp.SourceCompiler.IConstructor>()!;

        bool CompilerObject.Is<T>([NotNullWhen(true)] out T? result) where T : class
            => (result = this as T) is not null || UnderlyingObject.Is(out result);
    }
}
