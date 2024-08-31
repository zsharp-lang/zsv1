namespace ZSharp.VM.Types
{
    public class PrimitiveType(string name, ZSObject type) : ZSObject(type)
    {
        public string Name { get; } = name;

        public PrimitiveType(string name)
            : this(name, TypeSystem.Type) { }

        public override string ToString()
        {
            return Name;
        }
    }
}
