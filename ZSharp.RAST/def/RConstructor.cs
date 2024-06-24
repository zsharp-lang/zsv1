namespace ZSharp.RAST
{
    public sealed class RConstructor : RFunction
    {
        public RConstructor(string name, RSignature signature)
            : base(name, signature) { }

        public RConstructor(RId id, RSignature signature) 
            : base(id, signature)
        {
        }
    }
}
