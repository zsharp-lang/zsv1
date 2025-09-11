namespace ZSharp.Compiler
{
    public class ErrorMessage(string message)
        : Error
    {
        public string Message { get; } = message;

        public override string ToString()
            => Message;
    }
}
