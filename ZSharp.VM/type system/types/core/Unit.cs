namespace ZSharp.VM.Types
{
    internal class Unit() : PrimitiveType("Unit")
    {
        public override bool? IsAssignableFrom(Interpreter interpreter, ZSObject source)
        {
            return source == this;
        }

        public override bool? IsAssignableTo(Interpreter interpreter, ZSObject target)
        {
            return target == this;
        }
    }
}
