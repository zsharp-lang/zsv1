using CommonZ.Utils;

using Args = CommonZ.Utils.Collection<ZSharp.Objects.CompilerObject>;
using KwArgs = CommonZ.Utils.Mapping<string, ZSharp.Objects.CompilerObject>;


namespace ZSharp.Objects
{
    public interface ISignature : CompilerObject
    {
        public IEnumerable<IParameter>? GetArgs(Compiler.Compiler compiler);

        public IVarParameter? GetVarArgs(Compiler.Compiler compiler);

        public IEnumerable<IParameter>? GetKwArgs(Compiler.Compiler compiler);

        public IKeywordVarParameter? GetVarKwArgs(Compiler.Compiler compiler);

        public Mapping<CompilerObject, CompilerObject> MatchArguments(Compiler.Compiler compiler, Compiler.Argument[] arguments)
        {
            var (args, kwArgs) = Utils.SplitArguments(arguments);

            try
            {
                return MatchArguments(compiler, args, kwArgs);
            } catch (ArgumentsCountMismatchException argumentsCountMismatch)
            {
                throw new ArgumentMismatchException(this, arguments, innerException: argumentsCountMismatch);
            } catch (Compiler.InvalidCastException invalidCast)
            {
                throw new ArgumentMismatchException(this, arguments, innerException: invalidCast);
            }
        }

        public Mapping<CompilerObject, CompilerObject> MatchArguments(Compiler.Compiler compiler, Args args, KwArgs kwArgs)
        {
            var @params = GetArgs(compiler)?.ToList() ?? [];
            var varParams = GetVarArgs(compiler);
            var kwParams = GetKwArgs(compiler)?.ToList() ?? [];
            var varKwParams = GetVarKwArgs(compiler);

            if (args.Count > @params.Count && varParams is null)
                throw new ArgumentsCountMismatchException($"Expected {@params.Count} positional arguments but got {args.Count}.");

            if (kwArgs.Count > kwParams.Count && varKwParams is null)
                throw new ArgumentsCountMismatchException($"Expected {kwParams.Count} named arguments but got {kwArgs.Count}.");

            Mapping<CompilerObject, CompilerObject> result = [];

            var positionalArgumentsQueue = new Queue<CompilerObject>(args);

            foreach (var param in @params)
            {
                if (!positionalArgumentsQueue.TryDequeue(out var arg))
                    arg = param.Default ?? throw new ArgumentsCountMismatchException($"Expected {@params.Count} positional arguments but got {args.Count}");

                if (param.MatchArgument(compiler, arg).Ok(out var argumentResult))
                    result[param] = argumentResult;
                else throw new Compiler.InvalidCastException(arg, null!);
            }

            if (varParams is not null)
                result[varParams] = varParams.MatchArguments(compiler, [.. positionalArgumentsQueue]);
            else if (positionalArgumentsQueue.Count > 0)
                throw new ArgumentsCountMismatchException($"Expected {@params.Count} positional arguments but got {args.Count}");

            var keywordArguments = kwArgs.ToDictionary();

            foreach (var kwParam in kwParams)
            {
                if (!kwArgs.TryGetValue(kwParam.Name, out var kwArg))
                    kwArg = kwParam.Default ?? throw new ArgumentsCountMismatchException($"Expected {@params.Count} positional arguments but got {args.Count}");
                else
                    keywordArguments.Remove(kwParam.Name);

                result[kwParam] = kwArg;
            }

            if (varKwParams is not null)
                result[varKwParams] = varKwParams.MatchArguments(compiler, keywordArguments);
            else if (keywordArguments.Count > 0)
                throw new ArgumentsCountMismatchException($"Expected {@params.Count} named arguments but got {args.Count}");

            return result;
        }
    }
}
