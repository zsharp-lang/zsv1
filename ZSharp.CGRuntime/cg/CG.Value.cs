using ZSharp.RAST;

namespace ZSharp.CGRuntime
{
    public static partial class CG
    {
        public static HLVM.Dup Dup()
            => new();

        public static HLVM.Object Object(CGObject @object)
            => new(@object);

        public static HLVM.Literal Literal(object value, RLiteralType type)
            => new(value, type);

        public static HLVM.Literal String(string value)
            => HLVM.Literal.String(value);

        public static HLVM.Literal Integer(int value)
            => HLVM.Literal.Integer(value);

        public static HLVM.Literal Real(double value)
            => HLVM.Literal.Real(value);

        public static HLVM.Literal F32(float value)
            => HLVM.Literal.F32(value);

        public static HLVM.Literal F64(double value)
            => HLVM.Literal.F64(value);

        public static HLVM.Literal Boolean(bool value)
            => HLVM.Literal.Boolean(value);

        public static HLVM.Literal Null()
            => HLVM.Literal.Null();

        public static HLVM.Literal Unit()
            => HLVM.Literal.Unit();

        public static HLVM.Literal I8(sbyte value)
            => HLVM.Literal.I8(value);

        public static HLVM.Literal I16(short value)
            => HLVM.Literal.I16(value);

        public static HLVM.Literal I32(int value)
            => HLVM.Literal.I32(value);

        public static HLVM.Literal I64(long value)
            => HLVM.Literal.I64(value);

        public static HLVM.Literal U8(byte value)
            => HLVM.Literal.U8(value);

        public static HLVM.Literal U16(ushort value)
            => HLVM.Literal.U16(value);

        public static HLVM.Literal U32(uint value)
            => HLVM.Literal.U32(value);

        public static HLVM.Literal U64(ulong value)
            => HLVM.Literal.U64(value);

        public static HLVM.Literal Imaginary(double value)
            => HLVM.Literal.Imaginary(value);

        public static HLVM.Literal I(int value)
            => HLVM.Literal.I(value);

        public static HLVM.Literal U(uint value)
            => HLVM.Literal.U(value);
    }
}
