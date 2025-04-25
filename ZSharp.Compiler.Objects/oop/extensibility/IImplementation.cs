using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Objects
{
    public interface IImplementation
        : CompilerObject
    {
        public bool Implements(Compiler.Compiler compiler, CompilerObject specification, [NotNullWhen(true)] out IImplementsSpecification? implementation);
    }
}
