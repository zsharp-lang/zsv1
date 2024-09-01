namespace ZSharp.Resolver
{
    public static partial class Resolver
    {
        public static RFunction Resolve(Function function)
            => new(function.Name, Resolve(function.Signature))
            {
                Body = function.Body is null ? null : Resolve(function.Body),
                ReturnType = function.ReturnType is null ? null : Resolve(function.ReturnType)
            };

        public static RSignature Resolve(Signature signature)
            => new()
            {
                Args = signature.Args is null ? null : new(signature.Args.Select(Resolve)),
                KwArgs = signature.KwArgs is null ? null : new(signature.KwArgs.Select(Resolve)),
                VarArgs = signature.VarArgs is null ? null : Resolve(signature.VarArgs),
                VarKwArgs = signature.VarKwArgs is null ? null : Resolve(signature.VarKwArgs)
            };

        public static RParameter Resolve(Parameter parameter)
            => new(
                parameter.Name,
                parameter.Type is null ? null : Resolve(parameter.Type),
                parameter.Initializer is null ? null : Resolve(parameter.Initializer)
            );
    }
}
