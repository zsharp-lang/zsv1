namespace ZSharp.Compiler
{
    public sealed class Argument(CompilerObject @object)
    {
        public CompilerObject Object { get; set; } = @object;

        public string? Name { get; set; }

        public Argument(string? name, CompilerObject @object)
            : this(@object)
        {
            Name = name;
        }
    }
}
