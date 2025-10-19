using CommonZ.Utils;

namespace ZSharp.Compiler.Features.Callable
{
    public sealed class BoundSignature
    {
        public required CompilerObject SignatureObject { get; init; }

        public Collection<BoundParameter> Parameters { get; init; } = [];

        public static IResult<BoundSignature, Error> Create(Compiler compiler, CompilerObject @object, IArgumentStream arguments)
        {
            if (!@object.Is<ISignature>(out var signature))
                return Result<BoundSignature>.Error($"Object '{@object}' is not a signature.");

            var boundSignature = new BoundSignature
            {
                SignatureObject = @object,
            };
            foreach (var parameterObject in signature.Parameters(compiler))
            {
                if (!parameterObject.Is<IParameter>(out var parameter))
                    return Result<BoundSignature>.Error($"Object '{parameterObject}' is not a parameter.");

                if (
                    parameter.Match(compiler, arguments)
                    .When(out var value)
                    .Error(out var error)
                ) return Result<BoundSignature>.Error(error);

                boundSignature.Parameters.Add(
                    new()
                    {
                        ParameterObject = (CompilerObject)parameter,
                        ArgumentObject = value!,
                    }
                );
            }
            if (arguments.HasArguments)
                return Result<BoundSignature>.Error($"Too many arguments provided for signature '{@object}'.");

            return Result<BoundSignature>.Ok(boundSignature);
        }
    }
}
