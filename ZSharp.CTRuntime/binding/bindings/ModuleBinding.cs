//using CommonZ.Utils;

//namespace ZSharp.IR
//{
//    public sealed class ModuleBinding : IRBinding<Module>, IScope
//    {
//        static IType ModuleType = new NativeType("Module");

//        public Mapping<string, IBinding> Members { get; } = new();

//        public ModuleBinding(Module module) : base(module, ModuleType)
//        {
//            foreach (var function in module.Functions)
//            {
//                if (function.Name is null) continue;
//                Members.Add(function.Name, new FunctionBinding(function));
//            }
//        }
//    }
//}
