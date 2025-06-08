using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public partial class GenericFunction
    {
        public Result<GenericFunctionInstance, Error> CreateGenericInstance(Compiler.Compiler compiler, IType[] arguments)
        {
            if (arguments.Length != GenericParameters.Count)
                return Result<GenericFunctionInstance, Error>.Error(
                    $"Invalid generic argument count: Expected {GenericParameters.Count}, got {arguments.Length}"
                );

            Mapping<GenericParameter, IType> genericArguments = [];
            foreach (var (parameter, argument) in GenericParameters.Zip(arguments))
            {
                if (!parameter.Match(argument))
                    return Result<GenericFunctionInstance, Error>.Error(
                        $"Type {argument} cannot be assigned to generic parameter {parameter.Name}"
                    );

                genericArguments[parameter] = argument;
            }

            return Result<GenericFunctionInstance, Error>.Ok(
                new(this)
                {
                    Context = new()
                }
            );
        }
    }
}
