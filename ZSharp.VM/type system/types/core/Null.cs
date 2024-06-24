namespace ZSharp.VM.Types
{
    internal class Null() : PrimitiveType("NullType")
    {
        public override bool? IsAssignableFrom(Interpreter interpreter, ZSObject source)
        {
            return false;
        }

        public override bool? IsAssignableTo(Interpreter interpreter, ZSObject target)
        {
            return target is Nullable;
        }
    }
}
