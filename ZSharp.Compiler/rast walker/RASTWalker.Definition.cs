using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal partial class RExpressionWalker<T, V>
    {
        public IEnumerable<T> Walk(RFunction function)
            => [
                ..(function.Signature.Args ?? []).SelectMany(Walk),
                ..Walk(function.Signature.VarArgs),
                ..(function.Signature.KwArgs ?? []).SelectMany(Walk),
                ..Walk(function.Signature.VarKwArgs),
                ..Walk(function.Body)
            ];

        public IEnumerable<T> Walk(RLetDefinition let)
            => [
                ..Walk(let.Type),
                ..Walk(let.Value)
            ];

        public IEnumerable<T> Walk(RModule module)
            => [
                ..(module.Content?.Statements ?? []).SelectMany(Walk)
            ];

        public IEnumerable<T> Walk(ROOPDefinition oop)
            => [
                ..(oop.Bases ?? []).SelectMany(Walk),
                ..Walk(oop.Content),
            ];

        public IEnumerable<T> Walk(RParameter? parameter)
            => parameter is null ? [] : [
                ..Walk(parameter.Type),
                ..Walk(parameter.Default)
            ];

        public IEnumerable<T> Walk(RVarDefinition var)
            => [
                ..Walk(var.Type),
                ..Walk(var.Value)
            ];
    }
}
