using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class StubParameter(string name)
        : CompilerObject
        , IParameter
        , ITyped
        , IReferencable<IParameter>
    {
        public string Name { get; set; } = name;

        public CompilerObject? Default { get; set; }

        public IType? Type { get; set; }

        #region Parameter

        CompilerObjectResult IParameter.MatchArgument(Compiler.Compiler compiler, CompilerObject argument)
        {
            if (Type is null)
                return CompilerObjectResult.Error($"Parameter {Name} does not have a type");

            return compiler.TypeSystem.ImplicitCast(argument, Type);
        }

        #endregion

        #region Typed

        IType ITyped.Type => Type ?? throw new InvalidOperationException($"Parameter {Name} does not define a type");

        #endregion

        #region Reference

        IParameter IReferencable<IParameter>.CreateReference(Referencing @ref, ReferenceContext context)
            => new StubParameter(Name)
            {
                Default = Default is null ? null : @ref.CreateReference(Default, context),
                Type = Type is null ? null : @ref.CreateReference<IType>(Type, context),
            };

        #endregion
    }
}
