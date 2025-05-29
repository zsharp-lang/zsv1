using System;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class Nullable(IType underlyingType)
        : CompilerObject
        , IRTCastFrom
        , IRTCastTo
        , IType
    {
        public IType UnderlyingType { get; set; } = underlyingType;

        bool IType.IsEqualTo(Compiler.Compiler compiler, IType other)
        {
            if (other is not Nullable nullable) return false;
            return compiler.TypeSystem.AreEqual(UnderlyingType, nullable.UnderlyingType);
        }

        Result<TypeCast, Error> IRTCastFrom.Cast(Compiler.Compiler compiler, CompilerObject value)
        {
            var innerCastResult = compiler.CG.Cast(value, UnderlyingType);

            TypeCast innerCast;

            if (innerCastResult.Error(out var error))
                return Result<TypeCast, Error>.Error(error);
            else innerCast = innerCastResult.Unwrap();

            var innerCastCodeResult = compiler.IR.CompileCode(innerCast.Cast);

            IRCode innerCastCode;

            if (innerCastCodeResult.Error(out error))
                return Result<TypeCast, Error>.Error(error);
            else innerCastCode = innerCastCodeResult.Unwrap();

            return Result<TypeCast, Error>.Ok(
                new()
                {
                    Cast = new RawCode(new([
                            .. innerCastCode.Instructions,
                            .. (IR.VM.Instruction[])(innerCast.CanFail ? [
                                innerCast.OnFail,
                                new IR.VM.PutNull(),
                            ] : []),
                        ])
                    {
                        Types = [this]
                    }
                    ),
                    OnCast = innerCast.OnCast
                }
            );
        }

        Result<TypeCast, Error> IRTCastTo.Cast(Compiler.Compiler compiler, CompilerObject value, IType targetType)
        {
            if (targetType is not Nullable nullable)
                return Result<TypeCast, Error>.Error(
                    "Nullable types can only be cast to other nullable types"
                );

            var innerCastResult = compiler.CG.Cast(value, nullable.UnderlyingType);

            TypeCast innerCast;

            if (innerCastResult.Error(out var error))
                return Result<TypeCast, Error>.Error(error);
            else innerCast = innerCastResult.Unwrap();

            var innerCastCodeResult = compiler.IR.CompileCode(innerCast.Cast);

            IRCode innerCastCode;

            if (innerCastCodeResult.Error(out error))
                return Result<TypeCast, Error>.Error(error);
            else innerCastCode = innerCastCodeResult.Unwrap();

            return Result<TypeCast, Error>.Ok(
                new()
                {
                    Cast = new RawCode(new([
                            .. innerCastCode.Instructions,
                            .. (IR.VM.Instruction[])(innerCast.CanFail ? [
                                innerCast.OnFail,
                                new IR.VM.PutNull(),
                            ] : []),
                        ])
                        {
                            Types = [targetType]
                        }
                    ),
                    OnCast = innerCast.OnCast
                }
            );
        }
    }
}
