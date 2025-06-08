namespace ZSharp.Compiler
{
    public class Argument_NEW<T>(T value)
        where T : CompilerObject
    {
        public T Value { get; set; } = value;

        public string? Name { get; set; }

        public Argument_NEW(string? name, T value)
            : this(value)
        {
            Name = name;
        }
    }
}
