using CommonZ.Utils;
using ZSharp.Compiler;

using Args = CommonZ.Utils.Collection<ZSharp.Objects.CompilerObject>;
using KwArgs = CommonZ.Utils.Mapping<string, ZSharp.Objects.CompilerObject>;


namespace ZSharp.Objects
{
    public interface ISignature 
        : CompilerObject
        , IType
        , ITypeAssignableToType
    {
        public IEnumerable<IParameter> Args { get; }

        public IVarParameter? VarArgs { get; }

        public IEnumerable<IParameter> KwArgs { get; }

        public IVarParameter? VarKwArgs { get; }

        public IType ReturnType { get; }

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
            var @params = Args.ToList();
            var kwParams = KwArgs.ToList();

            if (args.Count > @params.Count && VarArgs is null)
                throw new ArgumentsCountMismatchException($"Expected {@params.Count} positional arguments but got {args.Count}.");

            if (kwArgs.Count > kwParams.Count && VarKwArgs is null)
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

            if (VarArgs is not null)
                result[VarArgs] = VarArgs.MatchArguments(compiler, [.. positionalArgumentsQueue.Select(arg => new Compiler.Argument(arg))]);
            else if (positionalArgumentsQueue.Count > 0)
                throw new ArgumentsCountMismatchException($"Expected {@params.Count} positional arguments but got {args.Count}");

            var varKeywordArguments = new Collection<Compiler.Argument>();

            foreach (var kwParam in kwParams)
            {
                if (!kwArgs.Remove(kwParam.Name, out var kwArg))
                    kwArg = kwParam.Default ?? throw new ArgumentsCountMismatchException($"Expected {@params.Count} positional arguments but got {args.Count}");
                else varKeywordArguments.Add(new(kwParam.Name, kwArg));

                result[kwParam] = kwArg;
            }

            if (VarKwArgs is not null)
                result[VarKwArgs] = VarKwArgs.MatchArguments(compiler, [.. varKeywordArguments]);
            else if (kwArgs.Count > 0)
                throw new ArgumentsCountMismatchException($"Expected {@params.Count} named arguments but got {args.Count}");

            return result;
        }

        #region Protocols

        bool IType.IsEqualTo(Compiler.Compiler compiler, IType type)
        {
            if (type is not ISignature callableType)
                return false;

            if (Args.Count() != callableType.Args.Count())
                return false;

            if (
                VarArgs is null && callableType.VarArgs is not null ||
                VarArgs is not null && callableType.VarArgs is null
            )
                return false;
            if (
                !(VarArgs is null && callableType.VarArgs is null) &&
                !compiler.TypeSystem.AreEqual(VarArgs!.Type, callableType.VarArgs!.Type)
            )
                return false;

            if (KwArgs.Count() != callableType.KwArgs.Count())
                return false;

            if (
                VarKwArgs is null && callableType.VarKwArgs is not null ||
                VarKwArgs is not null && callableType.VarKwArgs is null
            )
                return false;
            if (
                !(VarKwArgs is null && callableType.VarKwArgs is null) &&
                !compiler.TypeSystem.AreEqual(VarKwArgs!.Type, callableType.VarKwArgs!.Type)
            )
                return false;

            if (!compiler.TypeSystem.AreEqual(ReturnType, callableType.ReturnType))
                return false;

            foreach (var (left, right) in Args.Zip(callableType.Args))
                if (!compiler.TypeSystem.AreEqual(left.Type, right.Type))
                    return false;

            var otherKwArgs = callableType.KwArgs.ToDictionary(param => param.Name);

            foreach (var kwArg in KwArgs)
                if (!otherKwArgs.TryGetValue(kwArg.Name, out var otherKwArg))
                    return false;
                else if (!compiler.TypeSystem.AreEqual(kwArg.Type, otherKwArg.Type))
                    return false;

            return true;
        }

        bool? ITypeAssignableToType.IsAssignableTo(Compiler.Compiler compiler, IType target)
        {
            if (target is not ISignature callableType)
                return false;

            if (Args.Count() != callableType.Args.Count())
                return false;

            if (
                VarArgs is null && callableType.VarArgs is not null ||
                VarArgs is not null && callableType.VarArgs is null
            )
                return false;
            if (
                !(VarArgs is null && callableType.VarArgs is null) &&
                !compiler.TypeSystem.IsAssignableFrom(VarArgs!.Type, callableType.VarArgs!.Type)
            )
                return false;

            if (KwArgs.Count() != callableType.KwArgs.Count())
                return false;

            if (
                VarKwArgs is null && callableType.VarKwArgs is not null ||
                VarKwArgs is not null && callableType.VarKwArgs is null
            )
                return false;
            if (
                !(VarKwArgs is null && callableType.VarKwArgs is null) &&
                !compiler.TypeSystem.IsAssignableFrom(VarKwArgs!.Type, callableType.VarKwArgs!.Type)
            )
                return false;

            if (!compiler.TypeSystem.IsAssignableTo(ReturnType, callableType.ReturnType))
                return false;

            foreach (var (left, right) in Args.Zip(callableType.Args))
                if (!compiler.TypeSystem.IsAssignableFrom(left.Type, right.Type))
                    return false;

            var otherKwArgs = callableType.KwArgs.ToDictionary(param => param.Name);

            foreach (var kwArg in KwArgs)
                if (!otherKwArgs.TryGetValue(kwArg.Name, out var otherKwArg))
                    return false;
                else if (!compiler.TypeSystem.IsAssignableFrom(kwArg.Type, otherKwArg.Type))
                    return false;

            return true;
        }

        #endregion
    }
}
