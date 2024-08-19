using ZSharp.RAST;

namespace ZSharp.CGRuntime.HLVM
{
    public sealed class Literal(object value, RLiteralType type) : Instruction
    {
        public object Value { get; } = value;

        public RLiteralType Type { get; } = type;

        public static Literal String(string value)
            => new(value, RLiteralType.String);

        public static Literal Integer(int value)
            => new(value, RLiteralType.Integer);

        public static Literal Real(double value)
            => new(value, RLiteralType.Real);

        public static Literal F32(float value)
            => new(value, RLiteralType.F32);

        public static Literal F64(double value)
            => new(value, RLiteralType.F64);

        public static Literal Boolean(bool value)
            => new(value, RLiteralType.Boolean);

        public static Literal Null()
            => new(null!, RLiteralType.Null);

        public static Literal Unit()
            => new(null!, RLiteralType.Unit);

        public static Literal I8(sbyte value)
            => new(value, RLiteralType.I8);

        public static Literal I16(short value)
            => new(value, RLiteralType.I16);

        public static Literal I32(int value)
            => new(value, RLiteralType.I32);

        public static Literal I64(long value)
            => new(value, RLiteralType.I64);

        public static Literal U8(byte value)
            => new(value, RLiteralType.U8);

        public static Literal U16(ushort value)
            => new(value, RLiteralType.U16);

        public static Literal U32(uint value)
            => new(value, RLiteralType.U32);

        public static Literal U64(ulong value)
            => new(value, RLiteralType.U64);

        public static Literal Imaginary(double value)
            => new(value, RLiteralType.Imaginary);

        public static Literal I(int value)
            => new(value, RLiteralType.I);

        public static Literal U(uint value)
            => new(value, RLiteralType.U);
    }
}
