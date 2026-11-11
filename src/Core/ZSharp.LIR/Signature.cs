namespace ZSharp.LIR
{
    public sealed class Signature
    {
        public List<Parameter> Parameters { get; } = [];

        /// <summary>
        /// The returned type, or <see langword="null"/> if nothing is returned.
        /// </summary>
        public Type? ReturnType { get; set; }
    }
}
