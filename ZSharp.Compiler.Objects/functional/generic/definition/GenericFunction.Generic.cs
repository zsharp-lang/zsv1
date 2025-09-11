using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public partial class GenericFunction
    {
        public Result<GenericFunctionInstance> CreateGenericInstance(Compiler.Compiler compiler, IType[] arguments)
        {
            if (arguments.Length != GenericParameters.Count)
                return Result<GenericFunctionInstance>.Error(
                    $"Invalid generic argument count: Expected {GenericParameters.Count}, got {arguments.Length}"
                );

            Mapping<GenericParameter, IType> genericArguments = [];
            foreach (var (parameter, argument) in GenericParameters.Zip(arguments))
            {
                if (!parameter.Match(argument))
                    return Result<GenericFunctionInstance>.Error(
                        $"Type {argument} cannot be assigned to generic parameter {parameter.Name}"
                    );

                genericArguments[parameter] = argument;
            }

            ReferenceContext context = new();
            foreach (var (p, a) in genericArguments)
                context.CompileTimeValues.Cache(p, a);

            return Result<GenericFunctionInstance>.Ok(
                new()
                {
                    GenericArguments = genericArguments,
                    GenericFunction = this,
                    Signature = compiler.Feature<Referencing>().CreateReference<Signature>(Signature, context)
                }
            );
        }
    }
}
