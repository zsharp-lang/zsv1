namespace ZSharp.VM.Types
{
    public sealed class Nullable(ZSObject innerType) : PrimitiveType($"Nullable<{innerType}>")
    {
        public ZSObject InnerType { get; set; } = innerType;

        public override bool? IsAssignableTo(Interpreter interpreter, ZSObject target)
        {
            if (target is not Nullable nullable)
                return false;
            return interpreter.TypeSystem.IsAssignableTo(InnerType, nullable.InnerType);
        }

        public override bool? IsAssignableFrom(Interpreter interpreter, ZSObject source)
        {
            if (source == TypeSystem.Null) return true;
            if (interpreter.TypeSystem.IsAssignableFrom(InnerType, source)) return true;
            if (source is not Nullable nullable)
                return false;
            return interpreter.TypeSystem.IsAssignableFrom(InnerType, source);
        }
    }
}
