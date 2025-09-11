using CommonZ.Utils;
using System.Diagnostics.CodeAnalysis;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class InferredType
        : CompilerObject
        , IType
        , IImplicitCastFromValue
    {
        private readonly Compiler.Compiler compiler;

        public ITypeInferenceContext Context { get; }

        public required IInferredTypeResolver Resolver { get; set; }

        public IType? ResolvedType { get; private set; }

        public bool IsResolved => ResolvedType is not null;

        public Collection<IType> CollectedTypes { get; set; } = [];

        internal InferredType(Compiler.Compiler compiler, ITypeInferenceContext context)
        {
            this.compiler = compiler;
            Context = context;
        }

        [MemberNotNull(nameof(ResolvedType))]
        public IType Resolve()
            => ResolvedType = Resolver.Resolve(compiler, this);

        CompilerObject IImplicitCastFromValue.ImplicitCastFromValue(Compiler.Compiler compiler, CompilerObject value)
        {
            if (!compiler.TypeSystem.IsTyped(value, out var type))
                throw new NotImplementedException();

            CollectedTypes.Add(type);
            return this;
        }
    }
}
