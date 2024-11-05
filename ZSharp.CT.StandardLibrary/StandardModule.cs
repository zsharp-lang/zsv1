using ZSharp.IR;
using ZSharp.VM;

namespace ZSharp.CT.StandardLibrary
{
    public sealed class StandardModule : InternalModule
    {
        private Module? module { get; set; }

        public CGObjects.RTFunction Print { get; private set; } = null!;

        public CGObjects.CTFunction Import { get; private set; } = null!;

        public CGObjects.SimpleFunctionOverloadGroup AdditionOperator { get; private set; } = null!;

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

            AdditionOperator = new("+");

            InternalFunction addFloat32, addInt32;

            addFloat32 = StandardFunctions.AddFloat32(runtime);
            FunctionImplementations[addFloat32.IR!] = addFloat32.Implementation;

            module.Functions.Add(addFloat32.IR!);

            addInt32 = StandardFunctions.AddInt32(runtime);
            FunctionImplementations[addInt32.IR!] = addInt32.Implementation;

            module.Functions.Add(addInt32.IR!);

            AdditionOperator.Overloads.AddRange([
                addFloat32,
                addInt32,
            ]);

            var moduleCG = new CGObjects.Module(module.Name!);

            StringImporter.AddImporter("std", StandardLibraryImporter);
            StandardLibraryImporter.AddModule(moduleCG, "io");
            Import = new ImportFunction(StringImporter);

            moduleCG.Members.Add("print", Print);

            return module;
        }
    }
}
