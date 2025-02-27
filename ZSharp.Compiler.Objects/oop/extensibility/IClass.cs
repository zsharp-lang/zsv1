namespace ZSharp.Objects
{
    public interface IClass
    {
        public string Name { get; set; }

        public IClass? Base { get; set; }

        public virtual void OnDerivation(IClass derived) { }
    }
}
