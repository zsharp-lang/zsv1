namespace ZSharp.RAST
{
    public sealed class RId : RExpression
    {
        public string Name { get; set; }

        public RId(string name)
        {
            Name = name;
        }
    }
}
