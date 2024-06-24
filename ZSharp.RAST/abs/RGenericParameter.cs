namespace ZSharp.RAST
{
    public class RGenericParameter : RNode
    {
        public string Name { get; set; }

        public RGenericParameter(string name)
        {
            Name = name;
        }
    }
}
