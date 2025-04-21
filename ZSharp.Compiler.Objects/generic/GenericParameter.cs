using ZSharp.Compiler;
using ZSharp.IR;

namespace ZSharp.Objects
{
    public sealed class GenericParameter
        : CompilerObject
        , ICompileIRType<IR.GenericParameter>
        , IReferencable
    {
        public string Name { get; set; } = string.Empty;

        public IR.GenericParameter? IR { get; set; }

        public IR.GenericParameter CompileIRType(Compiler.Compiler compiler)
            => IR ??= new(Name);

        CompilerObject IReferencable.CreateReference(Referencing @ref, ReferenceContext context)
            => context.CompileTimeValues.Cache(this, out var result) ? result : this;
    }
}
