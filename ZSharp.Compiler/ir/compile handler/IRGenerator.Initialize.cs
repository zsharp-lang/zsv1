//using ZSharp.CGObjects;

//namespace ZSharp.Compiler
//{
//    internal partial class IRGenerator
//    {
//        private void Initialize(Function function)
//        {

//        }

//        private void Initialize(Global global)
//        {
//            if (CurrentContext is not Module module)
//                throw new Exception("Global definition must be in a module context");

//            if (global.Type is not null)
//                AddDependencies(DependencyState.Defined, global.Type);
//            else if (global.Initializer is not null)
//                AddDependencies(DependencyState.Declared, global.Initializer);
//            else throw new Exception();

//            global.Module = module;
//        }

//        private void Initialize(Module module)
//        {
//            Module? parent = CurrentContext as Module;

//            if (CurrentContext is not null && parent is null)
//                throw new Exception("Module definition must be in a module or a top level context");

//            var ir = module.IR = new IR.Module(module.Name);

//            ir.Functions.Add(module.InitFunction.IR = new(RuntimeModule.TypeSystem.Void));

//            if (parent is not null)
//                parent.IR!.Submodules.Add(ir);

//            using (ContextOf(module))
//                foreach (var contentObject in runtime.Run(module.Content))
//                    throw new Exception("top-level code must not generate RT code!");
//        }
//    }
//}
