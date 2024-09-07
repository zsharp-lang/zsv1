using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public sealed class Module(string name)
        : CGObject
        , ICTGetMember<MemberName>
        , ICTReadable
    {
        IR.IType ICTReadable.Type => throw new NotImplementedException();

        public Mapping<string, CGObject> Members { get; } = [];

        public Collection<CGObject> ImportedMembers { get; } = [];

        public IR.Module? IR { get; set; }

        public RTFunction InitFunction { get; } = new(null);

        public string? Name { get; set; } = name;

        public CGCode Content { get; } = [];

        public CGObject Member(ICompiler compiler, string member)
            => Members[member];

        Code ICTReadable.Read(ICompiler compiler)
            => new([
                new IR.VM.GetObject(IR!)
            ])
            {
                Types = [null!], // TODO: fix type
            };
    }
}
