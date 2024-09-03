namespace ZSharp.VM
{
    public class TypeError : ZSRuntimeException
    {
        public TypeError(string message) : base(message) { }
    }
}
