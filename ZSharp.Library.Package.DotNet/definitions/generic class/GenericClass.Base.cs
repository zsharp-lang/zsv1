namespace Package.DotNet
{
    partial class GenericClass
        : ISingleInheritance
    {
        public CompilerObject? Base { get; set; }

        public List<CompilerObject> Interfaces { get; } = [];
    }
}
