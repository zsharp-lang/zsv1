namespace ZSharp.Objects
{
    public interface IOOPMember : CompilerObject
    {
        public IOOPType Owner { get; }

        public bool IsInstance { get; }

        public bool IsClass { get; }

        public bool IsStatic { get; }
    }
}
