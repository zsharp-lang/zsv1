using ZSharp.Objects;

namespace ZSharp.ZSSourceCompiler
{
    public sealed class ClassCompiler(
        ZSSourceCompiler compiler, 
        OOPDefinition node, 
        ClassMetaClass metaClass
    )
        : ContextCompiler<OOPDefinition, Class>(
            compiler, node, new()
            {
                Name = node.Name
            }
        )
    {
        public override Class Compile()
        {
            if (Node.Bases is not null)
            {
                var bases = Node.Bases.Select(Compiler.CompileType).ToArray();

                if (bases.Length > 0)
                {
                    int interfacesIndex = 0;
                    if (bases[0] is IClass @base)
                        Object.Base = (IClass)bases[interfacesIndex++];

                    for (; interfacesIndex < bases.Length; interfacesIndex++)
                        Object.Interfaces.Add(bases[interfacesIndex]);
                }
            }

            var bodyCompiler = new ClassBodyCompiler(Compiler, Node, Object);

            using (Context.Compiler(this))
            using (Context.Scope(Object))
            {
                bodyCompiler.Compile();
                bodyCompiler.CompileSinglePass();
            }
            
            foreach (var @interface in Object.Interfaces)
                if (@interface is IAbstraction abstraction)
                    ResolveImplementation(abstraction);

            using (Context.Compiler(this))
            using (Context.Scope(Object))
                bodyCompiler.CompileUntilComplete();

            return base.Compile();
        }

        private void ResolveImplementation(IAbstraction abstraction)
        {
            Implementation? result = null;
            foreach (var implementationInfo in Object.InterfaceImplementations)
                if (implementationInfo.Abstract == abstraction)
                {
                    result = implementationInfo;
                    break;
                }
            if (result is null)
                Object.InterfaceImplementations.Add(result = new(abstraction, Object));

            foreach (var specification in abstraction.Specifications)
                if (result.Mapping.ContainsKey(specification))
                    continue;
                else if (ResolveImplementation(specification) is not CompilerObject implementation)
                    Compiler.LogError(
                        $"Class {Object.Name} does not implement member {specification} in {abstraction}",
                        Node
                    );
                else
                {
                    result.Mapping.Add(specification, implementation);
                    if (implementation is IImplementsSpecification implementsSpecification)
                        implementsSpecification.OnImplementSpecification(Compiler.Compiler, abstraction, specification);
                }
        }

        private CompilerObject? ResolveImplementation(CompilerObject specification)
        {
            if (!Compiler.Compiler.TypeSystem.IsTyped(specification, out var specificationType))
            {
                Compiler.LogError(
                    $"{specification} is an invalid specification object",
                    Node
                );

                return null;
            }

            List<CompilerObject> possibleImplementations = [];
            List<string> errors = [];

            foreach (var item in Object.Content)
                if (Compiler.Compiler.TypeSystem.ImplicitCast(item, specificationType).Ok(out var implementation))
                    if (!Compiler.Compiler.TypeSystem.IsTyped(implementation, out var implementationType))
                        errors.Add($"Member {implementation} cannot implemenet {specification} because it is not typed");
                    else if (!Compiler.Compiler.TypeSystem.AreEqual(specificationType, implementationType))
                        errors.Add($"Member {implementation} cannot implement {specification} because it is not equal");
                    else possibleImplementations.Add(implementation);

            if (possibleImplementations.Count == 0)
            {
                Compiler.LogError(
                    string.Join(
                        "\n\t", [
                            $"Class {Object.Name} does not implement member {specification} in {specificationType}",
                            .. errors
                        ]
                    ),
                    Node
                );
                return null;
            }
            else if (possibleImplementations.Count > 1)
            {
                Compiler.LogError(
                    $"Class {Object.Name} has multiple implementations of {specification} in {specificationType}",
                    Node
                );
                return null;
            }
            
            return possibleImplementations[0];
        }
    }
}
