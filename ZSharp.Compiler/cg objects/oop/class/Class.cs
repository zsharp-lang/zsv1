using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public sealed class Class
        : CGObject
        , ICTGetMember<MemberName>
    {
        public Mapping<string, CGObject> Members { get; } = [];

        public IR.Class? IR { get; set; }

        public string? Name { get; set; }

        public Class? Base { get; set; }

        //public List<Interface>? Interfaces { get; set; }

        public CGObject Member(Compiler.Compiler compiler, MemberName member)
            => Members[member];
    }
}
