namespace ZSharp.VM.Types
{
    internal class Void() : PrimitiveType("Void")
    {
        public override bool? IsAssignableFrom(Interpreter interpreter, ZSObject source)
        {
            return false;
        }

        public override bool? IsAssignableTo(Interpreter interpreter, ZSObject target)
        {
            return false;
        }
    }
}
