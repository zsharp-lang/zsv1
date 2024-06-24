namespace ZSharp.IR.VM
{
    public sealed class PutString(string value) : Put
    {
        public string Value { get; set; } = value;
    }
}
