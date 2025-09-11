using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class StubVarParameter(string name)
        : CompilerObject
        , IVarParameter
        , ITyped
    {
        public string Name { get; set; } = name;

        public IType? Type { get; set; }

        #region Var Parameter

        CompilerObject IVarParameter.MatchArguments(Compiler.Compiler compiler, Collection<Argument> arguments)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Typed

        IType ITyped.Type => Type ?? throw new InvalidOperationException($"Variadic parameter {Name} does not define a type");

        #endregion
    }
}
