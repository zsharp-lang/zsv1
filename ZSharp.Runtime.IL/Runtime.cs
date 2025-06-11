using CommonZ.Utils;

namespace ZSharp.Runtime.NET
{
    public class Runtime
    {
        internal readonly IL2IR.ILLoader ilLoader;
        internal readonly IR2IL.IRLoader irLoader;

        private readonly Cache<Type, object> typeObjects = [];

        public Context Context { get; } = new();

        public Hooks Hooks { get; } = new();

        public IRCodeEvaluator Evaluator { get; }

        public IR.RuntimeModule RuntimeModule { get; }

        public Runtime(IR.RuntimeModule runtimeModule)
        {
            RuntimeModule = runtimeModule;

            ilLoader = new(Context, runtimeModule);
            irLoader = new(Context, runtimeModule);

            Evaluator = new(this);

            foreach (var (ir, il) in new (IR.IType, Type)[]
            {
                (runtimeModule.TypeSystem.Type, typeof(TypeObject)),

                (runtimeModule.TypeSystem.Void, typeof(void)),
                (runtimeModule.TypeSystem.Boolean, typeof(bool)),

                (runtimeModule.TypeSystem.Int32, typeof(int)),

                (runtimeModule.TypeSystem.Float32, typeof(float)),

                (runtimeModule.TypeSystem.Object,  typeof(object)),
                (runtimeModule.TypeSystem.String, typeof(string)),
            })
                Context.Cache(ir, il);
        }

        public IL.Module Import(IR.Module module)
        {
            if (!Context.Cache(module, out var result))
                Context.Cache(result = irLoader.LoadModule(module), module);

            return result;
        }

        public Type Import(IR.IType type)
            => irLoader.LoadType(type);

        public IR.IType Import(Type type)
            => Context.Cache(type, out IR.IType? result) ? result : ilLoader.LoadType(type);

        public IR.Module Import(IL.Module module)
        {
            if (!Context.Cache(module, out var result))
                result = ilLoader.LoadModule(module);

            //foreach (var (ir, il) in loadedModule.MethodImplementations)
            //    irLoader.Context.Callables.Cache(ir, il);

            //irLoader.Context.Modules.Cache(loadedModule.Result, module);

            return result;
        }

        public IR.Function Import(IL.MethodInfo method)
            => Context.Cache<IR.Function>(method)!;

        public object GetObject(Type type)
        {
            throw new NotSupportedException();
        }
    }
}
