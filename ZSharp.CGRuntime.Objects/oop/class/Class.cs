using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public sealed class Class
        : CGObject
        , ICTGetMember<MemberName>
    {
        private readonly Mapping<string, CGObject> members = [];

        public IR.Class? IR { get; set; }

        public string? Name { get; set; }

        public CGObject? MetaClass { get; set; }

        public List<CGObject> Bases { get; } = [];

        public CGObject Member(ICompiler compiler, MemberName member)
            => members[member];
    }
}
