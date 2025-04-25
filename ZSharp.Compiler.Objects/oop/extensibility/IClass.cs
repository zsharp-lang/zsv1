namespace ZSharp.Objects
{
    public interface IClass
        : CompilerObject
        , Compiler.IType
    {
        public string Name { get; set; }

        public IClass? Base { get; set; }

        public virtual void OnDerivation(IClass derived) { }
    }
}
