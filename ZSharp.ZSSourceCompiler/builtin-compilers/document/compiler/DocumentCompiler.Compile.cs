
namespace ZSharp.ZSSourceCompiler
{
    public sealed partial class DocumentCompiler
    {
        private Objects.Module Compile(Module node)
        {
            var compiler = Compiler.CreateModuleCompiler(node);

            if (node.Name != string.Empty)
                Context.CurrentScope.Set(node.Name, compiler.Object);

            var module = compiler.Compile();
            if (module.Name is not null && module.Name != string.Empty)
                Object.Members.Add(module.Name!, module);

            Object.Content.Add(module);

            return module;
        }
    }
}
