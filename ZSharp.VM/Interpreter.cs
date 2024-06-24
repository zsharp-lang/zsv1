using ZSharp.IR;

namespace ZSharp.VM
{
    public sealed class Interpreter
    {
        //private readonly Assembler _assembler;
        private readonly IRLoader _irLoader;
        private readonly RuntimeModule _runtime;
        private readonly TypeSystem _typeSystem;
        private readonly VirtualMachine _vm;

        public RuntimeModule Runtime => _runtime;

        public Interpreter(RuntimeModule runtime, int callStackSize = 1024)
        {
            _runtime = runtime;
            _typeSystem = new(this);
            _irLoader = new(_runtime);
            _vm = new(_irLoader, callStackSize);
        }

        #region MainAPI

        public ZSObject? Evaluate(Instruction[] code, int stackSize)
        {
            return _vm.Evaluate(code, stackSize);
        }

        public void Execute(Instruction[] code, int stackSize)
        {
            _vm.Execute(code, stackSize);
        }

        public ZSObject? Call(IR.Function function, params ZSObject[] args)
        {
            var code = new Instruction[args.Length + 2];

            for (var i = 0; i < args.Length; i++)
                code[i] = new(OpCode.Push, args[i]);

            code[^2] = new(OpCode.LoadObjectFromMetadata, function);

            code[^1] = new(OpCode.Call, args.Length);

            return Evaluate(code, args.Length);
        }

        public ZSObject? Call(ZSFunction function, params ZSObject[] args)
        {
            var code = new Instruction[args.Length + 2];

            for (var i = 0; i < args.Length; i++)
                code[i] = new(OpCode.Push, args[i]);

            code[^2] = new(OpCode.Push, function);

            code[^1] = new(OpCode.Call, args.Length);

            return Evaluate(code, args.Length + 1);
        }

        public ZSObject? Call(IR.Method method, params ZSObject[] args)
        {
            var code = new Instruction[args.Length + 2];

            for (var i = 0; i < args.Length; i++)
                code[i] = new(OpCode.Push, args[i]);

            code[^2] = new(OpCode.LoadObjectFromMetadata, method);

            code[^1] = 
                (method.Attributes & IR.FunctionAttributes.VirtualMethod) == IR.FunctionAttributes.VirtualMethod
                ? new(OpCode.Call, args.Length)
                : new(OpCode.CallVirtual, (null as object) ?? throw new NotImplementedException());

            return Evaluate(code, args.Length);
        }

        #endregion

        public Instruction[] Assemble(IEnumerable<IR.VM.Instruction> code)
            => _vm.IRLoader.Assembler.Assemble(code);

        public TypeSystem TypeSystem => _typeSystem;

        #region IR

        public ZSObject LoadIR(IR.IRObject ir)
            => _vm.IRLoader.Load(ir);

        public T LoadIR<T>(IR.IRObject ir)
            where T : ZSObject
            => LoadIR(ir) as T ?? throw new($"Object {ir} doesn't map to object of type {typeof(T).Name}");

        public ZSObject LoadType(IR.IType type)
            => _vm.IRLoader.Load(type);

        public ZSObject ReloadIR(IR.IRObject ir)
            => _vm.IRLoader.Reload(ir);

        public void UnloadIR(IR.IRObject ir) => _vm.IRLoader.Unload(ir);

        public static IR.IRObject GetIR(ZSObject value)
            => value is IIRObject ir ? ir.IR : throw new Exception("Invalid IR object");

        public static T? GetIR<T>(ZSObject value) 
            where T : class
            => GetIR(value) is T ir ? ir : throw new Exception();

        //public IR.IType GetIR(IType type)
        //    => throw new NotImplementedException();

        public ZSObject SetIR(IR.IRObject ir, ZSObject value)
        {
            _vm.IRLoader.Set(ir, value);
            return value;
        }

        #endregion

        #region Object

        public static IType AsType(ZSObject @object)
            => @object is IType type ? type : throw new Exception("Invalid type");

        public bool IsInstance(ZSObject @object, IType type)
            => type.IsInstance(@object);

        //public bool IsInstance(ZSObject @object, ZSClass @class)
        //    => Call(TypeSystem.TypeInternal.Methods.IsInstance, @class, @object);

        public bool IsInstance(ZSObject @object, ZSObject typeObject)
            => typeObject switch
            {
                IType type => IsInstance(@object, type),
                //ZSClass @class => IsInstance(@object, @class),
                _ => throw new ArgumentException("Must be a valid type object", nameof(typeObject))
            };

        public static void Resize(ZSStruct @struct, int newSize) 
            => @struct.Resize(newSize);

        public static ZSObject TypeOf(ZSObject @object)
            => @object.Type;

        #endregion
    }
}
