namespace ZSharp.Compiler
{
    public readonly struct Log<T>
    {
        public T Message { get; init; }

        public LogLevel Level { get; init; }

        public LogOrigin Origin { get; init; }

        public override string ToString()
            => $"[{Level}]: {Message} {(Origin.ToString() is string s && s != string.Empty ? "@ " + s : string.Empty)}";
    }
}
