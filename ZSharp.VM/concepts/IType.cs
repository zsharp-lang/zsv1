namespace ZSharp.VM
{
    public interface IType
    {
        public bool IsInstance(ZSObject @object)
            => @object.Type == this;

        public bool? IsAssignableTo(Interpreter interpreter, ZSObject target);

        public bool? IsAssignableFrom(Interpreter interpreter, ZSObject source);
    }
}
