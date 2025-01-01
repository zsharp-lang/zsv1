namespace ZSharp.ZSSourceCompiler
{
    public partial class ZSSourceCompiler
    {
        public DocumentCompiler CreateDocumentCompiler(AST.Document document, string path)
        {
            return new DocumentCompiler(this, document, new(path));
        }

        public ModuleCompiler CreateModuleCompiler(Module module)
        {
            var compiler = new ModuleCompiler(this, module);

            compiler.oopTypesCompilers.Add("class", (c, oop) =>
            {
                var metaClass = oop.Of is null ? null : c.CompileNode(oop.Of);

                // for C# types, we should create an uninitialized object via System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject
                // ConstructorInfo.Invoke has an overload which takes `this` as the first parameter (for late initialization).
                if (metaClass is not null) c.LogError("Metaclasses are not supported yet.", oop); 

                var @object = new Objects.Class()
                {
                    Name = oop.Name,
                };

                return new ClassCompiler(c, oop, @object);
            });

            compiler.oopTypesCompilers.Add("interface", (c, oop) =>
            {
                c.LogError("Interfaces are not supported yet.", oop);

                throw new NotImplementedException();
            });

            return compiler;
        }
    }
}
