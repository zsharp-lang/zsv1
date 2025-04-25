using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public interface ICallableType
        : IType
        , ITypeAssignableToType
    {
        public Collection<IType> Args { get; }

        public IType? VarArgs { get; }

        public Mapping<string, IType> KwArgs { get; }

        public IType? VarKwArgs { get; }

        public IType ReturnType { get; }

        #region Protocols

        bool IType.IsEqualTo(Compiler.Compiler compiler, IType type)
        {
            if (type is not ICallableType callableType)
                return false;

            if (Args.Count != callableType.Args.Count)
                return false;

            if (
                VarArgs is null && callableType.VarArgs is not null ||
                VarArgs is not null && callableType.VarArgs is null
            )
                return false;
            if (
                !(VarArgs is null && callableType.VarArgs is null) &&
                !compiler.TypeSystem.AreEqual(VarArgs!, callableType.VarArgs!)
            )
                return false;

            if (KwArgs.Count != callableType.KwArgs.Count)
                return false;

            if (
                VarKwArgs is null && callableType.VarKwArgs is not null ||
                VarKwArgs is not null && callableType.VarKwArgs is null
            )
                return false;
            if (
                !(VarKwArgs is null && callableType.VarKwArgs is null) &&
                !compiler.TypeSystem.AreEqual(VarKwArgs!, callableType.VarKwArgs!)
            )
                return false;

            if (!compiler.TypeSystem.AreEqual(ReturnType, callableType.ReturnType))
                return false;

            foreach (var (left, right) in Args.Zip(callableType.Args))
                if (!compiler.TypeSystem.AreEqual(left, right))
                    return false;

            foreach (var (name, kwArgType) in KwArgs)
                if (!callableType.KwArgs.TryGetValue(name, out var otherKwArgType))
                    return false;
                else if (!compiler.TypeSystem.AreEqual(kwArgType, otherKwArgType))
                    return false;

            return true;
        }

        bool? ITypeAssignableToType.IsAssignableTo(Compiler.Compiler compiler, IType target)
        {
            if (target is not ICallableType callableType)
                return null;

            if (Args.Count != callableType.Args.Count) return false;
            if (KwArgs.Count != callableType.KwArgs.Count) return false;

            if (
                VarArgs is null && callableType.VarArgs is not null ||
                VarArgs is not null && callableType.VarArgs is null
            )
                return false;
            if (
                !(VarArgs is null && callableType.VarArgs is null) &&
                !compiler.TypeSystem.IsAssignableFrom(VarArgs!, callableType.VarArgs!)
            )
                return false;

            if (
                VarKwArgs is null && callableType.VarKwArgs is not null ||
                VarKwArgs is not null && callableType.VarKwArgs is null
            )
                return false;
            if (
                !(VarKwArgs is null && callableType.VarKwArgs is null) &&
                !compiler.TypeSystem.IsAssignableFrom(VarKwArgs!, callableType.VarKwArgs!)
            )
                return false;

            if (!compiler.TypeSystem.IsAssignableTo(ReturnType, callableType.ReturnType))
                return false;

            foreach (var (left, right) in Args.Zip(callableType.Args))
                if (!compiler.TypeSystem.IsAssignableFrom(left, right))
                    return false;

            foreach (var (name, kwArgType) in KwArgs)
                if (!callableType.KwArgs.TryGetValue(name, out var otherKwArgType))
                    return false;
                else if (!compiler.TypeSystem.IsAssignableFrom(kwArgType, otherKwArgType))
                    return false;

            return true;
        }

        #endregion
    }
}
