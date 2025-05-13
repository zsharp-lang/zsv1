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
            using (Compiler.Compiler.ContextScope(new ClassContext(Object)))
            using (Context.Scope(Object))
            {
                bodyCompiler.Compile();
                bodyCompiler.CompileSinglePass();
            }
            
            foreach (var @interface in Object.Interfaces)
                if (@interface is IAbstraction abstraction)
                    ResolveImplementation(abstraction);

            using (Context.Compiler(this))
            using (Compiler.Compiler.ContextScope(new ClassContext(Object)))
            using (Context.Scope(Object))
                bodyCompiler.CompileUntilComplete();

            return base.Compile();
        }

        private void ResolveImplementation(IAbstraction abstraction)
        {
            if (Object is not IImplementsAbstraction implementsAbstraction)
                throw new();

            Implementation? result = null;
            foreach (var implementationInfo in Object.InterfaceImplementations)
                if (implementationInfo.Abstract == abstraction)
                {
                    result = implementationInfo;
                    break;
                }
            if (result is null)
                Object.InterfaceImplementations.Add(result = new(abstraction, Object));

            implementsAbstraction.ImplementAbstraction(Compiler.Compiler, abstraction, result);
        }
    }
}
