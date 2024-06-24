namespace ZSharp.VM.Types
{
    internal class Any() : PrimitiveType("Any")
    {
        public override bool? IsAssignableTo(Interpreter interpreter, ZSObject target)
        {
            return true;
        }

        public override bool? IsAssignableFrom(Interpreter interpreter, ZSObject source)
        {
            return true;
        }
    }
}
