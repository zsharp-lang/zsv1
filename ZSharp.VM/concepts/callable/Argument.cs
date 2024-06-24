namespace ZSharp.VM
{
    public class Argument(ZSObject value)
    {
        public string? Name { get; set; }

        public ZSObject Value { get; set; } = value;

        public Argument(string? name, ZSObject value) : this(value)
        {
            Name = name;
        }
    }
}
