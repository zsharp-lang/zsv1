using ZSharp.IR;
using ZSharp.VM;

namespace ZSharp.CT.StandardLibrary
{
    public sealed class StandardModule : InternalModule
    {
        private Module? module { get; set; }

        public CGObjects.RTFunction Print { get; private set; } = null!;

        public CGObjects.CTFunction Import { get; private set; } = null!;

        public StringImporter StringImporter { get; } = new();

        public StandardLibraryImporter StandardLibraryImporter { get; } = new();

        protected override Module Load(Runtime runtime)
            => module ??= LoadModule(runtime);

        private Module LoadModule(Runtime runtime)
        {
            Module module = new("io");

            InternalFunction print;
            Print = print = StandardFunctions.Print(runtime);
            FunctionImplementations[print.IR!] = print.Implementation;

            module.Functions.Add(print.IR!);

            var moduleCG = new CGObjects.Module(module.Name!);

            StringImporter.AddImporter("std", StandardLibraryImporter);
            StandardLibraryImporter.AddModule(moduleCG, "io");
            Import = new ImportFunction(StringImporter);

            moduleCG.Members.Add("print", Print);

            return module;
        }
    }
}
