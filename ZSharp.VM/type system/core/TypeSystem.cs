namespace ZSharp.VM
{
    public sealed class TypeSystem
    {
        public static ZSObject Type { get; } = new Types.Type();

        public static ZSObject Boolean { get; } = new Types.PrimitiveType("Boolean");

        public static ZSObject Function { get; } = new Types.PrimitiveType("Function");

        public static ZSObject Module { get; } = new Types.PrimitiveType("Module");

        public static ZSObject Null { get; } = new Types.PrimitiveType("NullType");

        public static ZSObject String { get; } = new Types.PrimitiveType("String");
    }
}
