using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public interface ICallableType
        : IType
        , ITypeAssignableToType
        , ISignature
    {
        public IType ReturnType { get; }

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
