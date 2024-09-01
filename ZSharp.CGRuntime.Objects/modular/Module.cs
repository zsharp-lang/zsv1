using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public sealed class Module(string? name)
        : CGObject
        , ICTGetMember<MemberName>
    {
        public Mapping<string, CGObject> Members { get; } = new();

        public IR.Module? IR { get; set; }

        public RTFunction InitFunction { get; } = new(null);

        public string? Name { get; set; } = name;

        public CGCode Content { get; } = [];

        public CGObject Member(ICompiler compiler, string member)
            => Members[member];
    }
}
