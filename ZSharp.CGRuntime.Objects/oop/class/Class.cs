using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public sealed class Class
        : CGObject
        , ICTGetMember<MemberName>
    {
        internal readonly Mapping<string, CGObject> members = [];

        public IR.Class? IR { get; set; }

        public string? Name { get; set; }

        public Class? Base { get; set; }

        //public List<Interface>? Interfaces { get; set; }

        public CGObject Member(ICompiler compiler, MemberName member)
            => members[member];
    }
}
