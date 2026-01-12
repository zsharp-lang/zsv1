namespace ZSharp.Importer.ILLoader
{
    public sealed partial class ILLoader
    {
        public ILLoader(Compiler.Compiler compiler, Platform.Runtime.Runtime runtime)
        {
            Compiler = compiler;
            Runtime = runtime;

            TypeSystem = new()
            {
                Array = new Objects.ArrayType(Runtime.TypeSystem.Array),
                Boolean = new Objects.BooleanType(Runtime.TypeSystem.Boolean),
                Char = new Objects.CharType(Runtime.TypeSystem.Char),
                Float16 = new Objects.Float16Type(Runtime.TypeSystem.Float16),
                Float32 = new Objects.Float32Type(Runtime.TypeSystem.Float32),
                Float64 = new Objects.Float64Type(Runtime.TypeSystem.Float64),
                Float128 = new Objects.Float128Type(Runtime.TypeSystem.Float128),
                Object = new Objects.ObjectType(Runtime.TypeSystem.Object),
                Pointer = new Objects.PointerType(Runtime.TypeSystem.Pointer),
                Reference = new Objects.ReferenceType(Runtime.TypeSystem.Reference),
                SInt8 = new Objects.SInt8Type(Runtime.TypeSystem.SInt8),
                SInt16 = new Objects.SInt16Type(Runtime.TypeSystem.SInt16),
                SInt32 = new Objects.SInt32Type(Runtime.TypeSystem.SInt32),
                SInt64 = new Objects.SInt64Type(Runtime.TypeSystem.SInt64),
                SIntNative = new Objects.SIntNativeType(Runtime.TypeSystem.SIntNative),
                String = new Objects.StringType(Runtime.TypeSystem.String),
                UInt8 = new Objects.UInt8Type(Runtime.TypeSystem.UInt8),
                UInt16 = new Objects.UInt16Type(Runtime.TypeSystem.UInt16),
                UInt32 = new Objects.UInt32Type(Runtime.TypeSystem.UInt32),
                UInt64 = new Objects.UInt64Type(Runtime.TypeSystem.UInt64),
                UIntNative = new Objects.UIntNativeType(Runtime.TypeSystem.UIntNative),
                Void = new Objects.VoidType(Runtime.TypeSystem.Void),
            };

            foreach (var (il, co) in (IEnumerable<(Type, CompilerObject)>)[
                (typeof(bool), TypeSystem.Boolean),
                (typeof(char), TypeSystem.Char),
                (typeof(Half), TypeSystem.Float16),
                (typeof(float), TypeSystem.Float32),
                (typeof(double), TypeSystem.Float64),
                (typeof(decimal), TypeSystem.Float128),
                (typeof(object), TypeSystem.Object),
                (typeof(sbyte), TypeSystem.SInt8),
                (typeof(short), TypeSystem.SInt16),
                (typeof(int), TypeSystem.SInt32),
                (typeof(long), TypeSystem.SInt64),
                (typeof(nint), TypeSystem.SIntNative),
                (typeof(string), TypeSystem.String),
                (typeof(byte), TypeSystem.UInt8),
                (typeof(ushort), TypeSystem.UInt16),
                (typeof(uint), TypeSystem.UInt32),
                (typeof(ulong), TypeSystem.UInt64),
                (typeof(nuint), TypeSystem.UIntNative),
                (typeof(void), TypeSystem.Void),
            ])
            {
                typeCache[il] = co;
            }

            if (runtime is not null)
                SetupExposeSystem();
        }
    }
}
