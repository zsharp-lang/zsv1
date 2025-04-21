namespace ZSharp.IR
{
    public sealed class CustomMetadata
    {
        public ICallable Constructor { get; set; }

        public object[] Arguments { get; set; }
    }
}
