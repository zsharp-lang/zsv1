using ZSharp.IR;
using ZSharp.VM;

namespace ZSharp.CT.StandardLibrary
{
    public sealed class StandardModule : InternalModule
    {
        private Module? module { get; set; }

        public CGObjects.RTFunction Print { get; private set; } = null!;

        protected override Module Load(Runtime runtime)
            => module ??= LoadModule(runtime);

        private Module LoadModule(Runtime runtime)
        {
            Module module = new("std");

            InternalFunction print;
            Print = print = StandardFunctions.Print(runtime);
            FunctionImplementations[print.IR!] = print.Implementation;

            module.Functions.Add(print.IR!);

            return module;
        }
    }
}
