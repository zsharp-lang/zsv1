using System.Diagnostics.CodeAnalysis;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class MethodOverloadGroup(string name)
        : OverloadGroup<IMethod>(name)
        , CompilerObject
        , ICTCallable
        , IImplementation
        , IRTBoundMember
    {
        CompilerObject IRTBoundMember.Bind(Compiler.Compiler compiler, CompilerObject value)
            => new BoundMethodOverloadGroup(this, value); // TODO: check that the value is a valid instance

        bool IImplementation.Implements(Compiler.Compiler compiler, CompilerObject specification, [NotNullWhen(true)] out IImplementsSpecification? implementation)
        {
            List<IImplementsSpecification> implementations = [];

            foreach (var method in Overloads)
                if (method is IImplementation implements && implements.Implements(compiler, specification, out var methodImplementation))
                    implementations.Add(methodImplementation);

            if (implementations.Count == 1)
                return (implementation = implementations[0]) is not null;

            return (implementation = null) is not null;
        }
    }
}
