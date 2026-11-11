namespace ZSharp.LIR
{
    /// <summary>
    /// Something a function reads and writes by name, either a
    /// <see cref="Parameter"/> or a <see cref="Local"/>.
    /// </summary>
    public interface Variable
    {
        public Type Type { get; }
    }
}
