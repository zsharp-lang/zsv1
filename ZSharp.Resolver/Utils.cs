namespace ZSharp.Resolver
{
    public static class Utils
    {
        public static RLiteral ParseLiteral(LiteralExpression literal)
        {
            var type = ResolveLiteralType(literal.Type);

            object value = type switch
            {
                RLiteralType.String => literal.Value,
                RLiteralType.Integer => DefaultIntegerType.Parse(literal.Value),
                RLiteralType.Real => DefaultRealType.Parse(literal.Value),
                RLiteralType.Boolean => 
                    literal.Value == "true" || (
                    literal.Value == "false"
                    ? false
                    : throw new Exception("How did we get here?")
                ),
                RLiteralType.Null => null!,
                RLiteralType.Unit => ValueTuple.Create(),
                RLiteralType.I8 => sbyte.Parse(literal.Value),
                RLiteralType.I16 => short.Parse(literal.Value),
                RLiteralType.I32 => int.Parse(literal.Value),
                RLiteralType.I64 => long.Parse(literal.Value),
                RLiteralType.U8 => byte.Parse(literal.Value),
                RLiteralType.U16 => ushort.Parse(literal.Value),
                RLiteralType.U32 => uint.Parse(literal.Value),
                RLiteralType.U64 => ulong.Parse(literal.Value),
                RLiteralType.I => IntPtr.Parse(literal.Value),
                RLiteralType.U => UIntPtr.Parse(literal.Value),
                RLiteralType.Imaginary => 
                    new System.Numerics.Complex(0, DefaultRealType.Parse(literal.Value)),
                _ => throw new NotImplementedException()
            };

            return
                literal.UnitType is null
                ? new(type, value)
                : new(type, value, Resolver.Resolve(literal.UnitType));
        }

        public static RLiteralType ResolveLiteralType(LiteralType type)
            => type switch
            {
                LiteralType.String => RLiteralType.String,
                _ => throw new NotImplementedException()
            };
    }
}
