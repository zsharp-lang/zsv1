namespace ZSharp.VM
{
    public class ZSRuntimeException : Exception
    {
        public ZSRuntimeException() { }

        public ZSRuntimeException(string message) : base(message) { }
    }
}
