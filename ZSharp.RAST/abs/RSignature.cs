namespace ZSharp.RAST
{
    public sealed class RSignature : RNode
    {
        public List<RParameter>? Args { get; set; }

        public RParameter? VarArgs { get; set; }

        public List<RParameter>? KwArgs { get; set; }

        public RParameter? VarKwArgs { get; set; }
    }
}
