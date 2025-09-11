using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class GenericParameter
        : CompilerObject
        , ICompileIRType<IR.GenericParameter>
        , IReferencable<IType>
        , IType
    {
        public string Name { get; set; } = string.Empty;

        public IR.GenericParameter? IR { get; set; }

        public IR.GenericParameter CompileIRType(Compiler.Compiler compiler)
            => IR ??= new(Name);

        IType IReferencable<IType>.CreateReference(Referencing @ref, ReferenceContext context)
            => context.CompileTimeValues.Cache<IType>(this, out var result) ? result : this;

        public bool Match(IType type)
            => true;
    }
}
