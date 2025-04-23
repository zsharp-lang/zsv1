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
                    if (bases[0] is GenericClassInstance)
                        Object.Base = (GenericClassInstance)bases[interfacesIndex++];
                }
            }

            using (Context.Compiler(this))
            using (Context.Scope(Object))
                new ClassBodyCompiler(Compiler, Node, Object).Compile();

            return base.Compile();
        }
    }
}
