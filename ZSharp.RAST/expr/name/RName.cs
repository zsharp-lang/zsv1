namespace ZSharp.RAST
{
    public abstract class RName(string name) : RExpression
    {
        public string Name { get; set; } = name;
    }
}
