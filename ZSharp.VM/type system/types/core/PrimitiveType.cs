namespace ZSharp.VM.Types
{
    public abstract class PrimitiveType(string name, ZSObject type) : ZSObject(type), IType
    {
        public string Name { get; } = name;

        public PrimitiveType(string name)
            : this(name, TypeSystem.Type) { }

        public override string ToString()
        {
            return Name;
        }

        public abstract bool? IsAssignableTo(Interpreter interpreter, ZSObject target);

        public abstract bool? IsAssignableFrom(Interpreter interpreter, ZSObject source);
    }
}
