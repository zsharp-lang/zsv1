namespace ZSharp.VM.Types
{
    internal class Boolean() : PrimitiveType("Boolean")
    {
        public override bool? IsAssignableTo(Interpreter interpreter, ZSObject target)
        {
            return target == this ? true : null;
        }

        public override bool? IsAssignableFrom(Interpreter interpreter, ZSObject source)
        {
            return source == this ? true : null;
        }
    }
}
