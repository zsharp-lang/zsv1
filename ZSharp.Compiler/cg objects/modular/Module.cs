using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class Module(string name)
        : CompilerObject
        , ICTGetMember<MemberName>
        , ICTReadable
        , ICompileIRObject<IR.Module, IR.Module>
    {
        CompilerObject ITyped.Type => throw new NotImplementedException();

        public Collection<CompilerObject> Content { get; } = [];

        public Mapping<string, CompilerObject> Members { get; } = [];

        public Collection<CompilerObject> ImportedMembers { get; } = [];

        public IR.Module? IR { get; set; }

        public RTFunction InitFunction { get; } = new(null);

        public string? Name { get; set; } = name;

        public CompilerObject Member(Compiler.Compiler compiler, string member)
            => Members[member];

        IRCode ICTReadable.Read(Compiler.Compiler compiler)
            => new([
                new IR.VM.GetObject(IR!)
            ])
            {
                Types = [null!], // TODO: fix type
            };

        public IR.Module CompileIRObject(Compiler.Compiler compiler, IR.Module? owner)
        {
            if (IR is not null)
                return IR;

            IR = new(Name);

            owner?.Submodules.Add(IR);

            foreach (var item in Content)
                compiler.CompileIRObject(item, IR);

            return IR;
        }
    }
}
