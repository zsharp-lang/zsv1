namespace ZSharp.Objects
{
    internal sealed class ArgumentsCountMismatchException : Exception
    {
        public ArgumentsCountMismatchException() : base() { }

        public ArgumentsCountMismatchException(string? message) : base(message) { }

        public ArgumentsCountMismatchException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
