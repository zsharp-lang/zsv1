using CommonZ.Utils;

namespace ZSharp.Runtime.NET
{
    public class Runtime
        : Interpreter.IRuntime
        , Interpreter.IHostLoader
    {
        private readonly IL2IR.ILLoader ilLoader;
        private readonly IR2IL.IRLoader irLoader;

        private readonly Cache<Type, object> typeObjects = [];

        public Context Context { get; } = new();

        public Interpreter.Interpreter Interpreter { get; }

        public Hooks Hooks { get; } = new();

        public Runtime(Interpreter.Interpreter interpreter)
        {
            Interpreter = interpreter;

            ilLoader = new(Context, interpreter.RuntimeModule);
            irLoader = new(Context, interpreter.RuntimeModule);

            interpreter.Compiler.Evaluators.Add(new IRCodeEvaluator(this));

            irLoader.GetObjectFunction = (loader, get) =>
            {
                if (get.IR is IR.IType type)
                {
                    loader.Output.Emit(IL.Emit.OpCodes.Ldtoken, Import(type));
                    loader.Output.Emit(IL.Emit.OpCodes.Call, Utils.GetMethod(Type.GetTypeFromHandle));
                    loader.Output.Emit(IL.Emit.OpCodes.Call, Hooks.GetObject);

                    loader.Push(typeof(object));
                }
            };

            foreach (var (ir, il) in new (IR.IType, Type)[]
            {
                (interpreter.RuntimeModule.TypeSystem.Type, typeof(TypeObject)),

                (interpreter.RuntimeModule.TypeSystem.Void, typeof(void)),
                (interpreter.RuntimeModule.TypeSystem.Boolean, typeof(bool)),

                (interpreter.RuntimeModule.TypeSystem.Int32, typeof(int)),

                (interpreter.RuntimeModule.TypeSystem.Float32, typeof(float)),

                (interpreter.RuntimeModule.TypeSystem.Object,  typeof(object)),
                (interpreter.RuntimeModule.TypeSystem.String, typeof(string)),
            })
                Context.Cache(ir, il);
        }

        void Interpreter.IRuntime.Import(IR.Module module)
            => Import(module);

        public IL.Module Import(IR.Module module)
        {
            if (!Context.Cache(module, out var result))
                Context.Cache(result = irLoader.LoadModule(module), module);

            return result;
        }

        public Type Import(IR.IType type)
            => irLoader.LoadType(type);

        public IR.IType Import(Type type)
            => Context.Cache(type, out IR.IType? result) ? result : throw new();

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
            if (typeObjects.Cache(type, out var result))
                return result;

            var ir = ilLoader.LoadType(type);
            var co = Interpreter.CompilerIRLoader.Import(ir);

            return typeObjects.Cache(type, new TypeObject(type, ir, co));
        }
    }
}
