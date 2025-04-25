using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        public bool IsAnonymous(CompilerObject @object)
            => (NameOf(@object) ?? string.Empty) == string.Empty;

        public string? NameOf(CompilerObject @object)
            => @object is INamedObject named ? named.Name : null;

        public bool NameOf(CompilerObject @object, [NotNullWhen(true)] out string? name)
            => (name = NameOf(@object)) is not null;
    }
}
