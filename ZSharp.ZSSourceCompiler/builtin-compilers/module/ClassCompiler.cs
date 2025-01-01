namespace ZSharp.ZSSourceCompiler
{
    public sealed class ClassCompiler(ZSSourceCompiler compiler, OOPDefinition oop, CompilerObject @object)
        : ContextCompiler<OOPDefinition, CompilerObject>(compiler, oop, @object)
    {
        public override CompilerObject Compile()
        {
            var metaClass = Node.Of is null ? null : Compiler.CompileNode(Node.Of);

            var cls = (Objects.Class)Object;

            if (Node.Bases is not null && Node.Bases.Count > 0)
                cls.Base = (Objects.Class)Compiler.Compiler.Evaluate(Compiler.CompileNode(Node.Bases[0]));

            return base.Compile();
        }
    }
}
