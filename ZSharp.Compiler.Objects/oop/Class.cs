namespace ZSharp.Objects
{
    public abstract class Class(Compiler.Compiler compiler, ClassSpec spec)
    {
        public abstract string Name { get; }

        public abstract Class Base { get; }
    }
}
