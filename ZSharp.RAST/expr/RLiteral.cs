﻿namespace ZSharp.RAST
{
    public enum RLiteralType
    {
        String,
        Integer,
        Real,
        True,
        False,
        Null,
        Unit,
        I8,
        I16,
        I32,
        I64,
        U8,
        U16,
        U32,
        U64,
        F32,
        F64,
        I,
        U,
        Imaginary,
    }

    public sealed class RLiteral(RLiteralType type, object value) : RExpression
    {
        public RLiteralType Type { get; } = type;

        public object Value { get; } = value;

        public RExpression? UnitType { get; }

        public RLiteral(RLiteralType type, object value, RExpression unitType)
            : this(type, value)
        {
            UnitType = unitType;
        }

        public T As<T>()
            => (T)Value!;

        public static RLiteral String(string value) => new(RLiteralType.String, value);

        public static RLiteral Integer(long value) => new(RLiteralType.Integer, value);

        public static RLiteral Real(double value) => new(RLiteralType.Real, value);

        public static RLiteral Boolean(bool value) => value ? True() : False();

        public static RLiteral True() => new(RLiteralType.True, true);

        public static RLiteral False() => new(RLiteralType.False, false);

        public static RLiteral Null() => new(RLiteralType.Null, null!);

        public static RLiteral Unit() => new(RLiteralType.Unit, null!);

        public static RLiteral I8(sbyte value) => new(RLiteralType.I8, value);

        public static RLiteral I16(short value) => new(RLiteralType.I16, value);

        public static RLiteral I32(int value) => new(RLiteralType.I32, value);

        public static RLiteral I64(long value) => new(RLiteralType.I64, value);

        public static RLiteral U8(byte value) => new(RLiteralType.U8, value);

        public static RLiteral U16(ushort value) => new(RLiteralType.U16, value);

        public static RLiteral U32(uint value) => new(RLiteralType.U32, value);

        public static RLiteral U64(ulong value) => new(RLiteralType.U64, value);

        public static RLiteral F32(float value) => new(RLiteralType.F32, value);

        public static RLiteral F64(double value) => new(RLiteralType.F64, value);

        public static RLiteral I(long value) => new(RLiteralType.I, value);

        public static RLiteral U(ulong value) => new(RLiteralType.U, value);

        public static RLiteral Imaginary(double value) => new(RLiteralType.Imaginary, value);

        public override string ToString() => Type == RLiteralType.Unit ? "()" : Value?.ToString() ?? "null";
    }
}
