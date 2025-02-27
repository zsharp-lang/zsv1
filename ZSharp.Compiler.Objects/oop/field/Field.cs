using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class Field(string name)
        : CompilerObject
        , ICTReadable
        , IRTBoundMember
        , ICompileIRObject<IR.Field, IR.Class>
    {
        public IR.Field? IR { get; set; }

        public string Name { get; set; } = name;

        public CompilerObject? Initializer { get; set; }

        public CompilerObject? Type { get; set; }

        public bool IsReadOnly { get; set; }

        public CompilerObject Bind(Compiler.Compiler compiler, CompilerObject value)
            => new BoundField(this, value);

        public IRCode Read(Compiler.Compiler compiler)
        {
            throw new NotImplementedException();
        }

        IR.Field ICompileIRObject<IR.Field, IR.Class>.CompileIRObject(Compiler.Compiler compiler, IR.Class? owner)
        {
            if (IR is not null)
            {
                if (owner is not null && IR.Owner is null)
                    owner.Fields.Add(IR);

                return IR;
            }

            IR = new(Name, compiler.CompileIRType(Type ?? throw new()));

            owner?.Fields.Add(IR);

            return IR;
        }
    }
}
