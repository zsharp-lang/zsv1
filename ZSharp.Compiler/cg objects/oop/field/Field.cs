using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public sealed class Field(string name)
        : CGObject
        , ICTReadable
        , IRTBoundMember
    {
        public IR.Field? IR { get; set; }

        public string Name { get; set; } = name;

        public CGObject? Initializer { get; set; }

        public CGObject? Type { get; set; }

        public CGObject Bind(Compiler.Compiler compiler, CGObject value)
            => new BoundField(this, value);

        public Code Read(Compiler.Compiler compiler)
        {
            throw new NotImplementedException();
        }
    }
}
