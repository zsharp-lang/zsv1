using System.Runtime.Intrinsics.Arm;
using ZSharp.Compiler;

namespace ZSharp.Interpreter
{
    public sealed partial class Interpreter
    {
        public IR.RuntimeModule RuntimeModule { get; }

        public Compiler.Compiler Compiler { get; }

        public Interpreter(IR.RuntimeModule? runtimeModule = null)
        {
            RuntimeModule = runtimeModule ?? IR.RuntimeModule.Standard;

            Compiler = new(RuntimeModule);
            Runtime = new(new()
            {
                Array = RuntimeModule.TypeSystem.Array,
                Boolean = RuntimeModule.TypeSystem.Boolean,
                Char = null!,
                Float16 = null!,
                Float32 = RuntimeModule.TypeSystem.Float32,
                Float64 = null!,
                Float128 = null!,
                Object = RuntimeModule.TypeSystem.Object,
                Pointer = RuntimeModule.TypeSystem.Pointer,
                Reference = RuntimeModule.TypeSystem.Reference,
                SInt8 = null!,
                SInt16 = null!,
                SInt32 = RuntimeModule.TypeSystem.Int32,
                SInt64 = null!,
                SIntNative = null!,
                String = RuntimeModule.TypeSystem.String,
                UInt8 = null!,
                UInt16 = null!,
                UInt32 = null!,
                UInt64 = null!,
                UIntNative = null!,
                Void = RuntimeModule.TypeSystem.Void
            });
            RTLoader = new()
            {
                TypeSystem = Runtime.TypeSystem
            };
            ILLoader = new(Compiler, Runtime);
        }
    }
}
