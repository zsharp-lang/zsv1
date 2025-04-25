namespace ZSharp.Objects
{
    public interface IImplementsAbstraction
        : CompilerObject
    {
        public Implementation ImplementAbstraction(Compiler.Compiler compiler, IAbstraction abstraction, Implementation? result = null)
        {
            result ??= new(abstraction, this);

            foreach (var specification in abstraction.Specifications)
            {
                if (result.Mapping.ContainsKey(specification))
                    continue;

                if (!compiler.NameOf(specification, out var specificationName))
                    continue; // this is actually invalid but ok

                var member = compiler.Member(this, specificationName);

                if (member is not IImplementation implementationCandidate)
                    throw new("Not implemented!"); // TODO: on implementation missing

                if (!implementationCandidate.Implements(compiler, specification, out var implementation))
                    throw new("Not implemented"); // TODO: on implementation missing

                result.Mapping.Add(specification, implementation);
                implementation.OnImplementSpecification(compiler, abstraction, specification);
            }

            return result;
        }
    }
}
