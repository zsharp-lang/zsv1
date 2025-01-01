using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class Field(string name)
        : CompilerObject
        , ICTReadable
        , IRTBoundMember
    {
        public IR.Field? IR { get; set; }

        public string Name { get; set; } = name;

        public CompilerObject? Initializer { get; set; }

        public CompilerObject? Type { get; set; }

        public CompilerObject Bind(Compiler.Compiler compiler, CompilerObject value)
            => new BoundField(this, value);

        public IRCode Read(Compiler.Compiler compiler)
        {
            throw new NotImplementedException();
        }
    }
}
