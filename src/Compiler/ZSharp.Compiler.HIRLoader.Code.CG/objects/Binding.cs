using Scope = ZSharp.Compiler.Current;

namespace ZSharp.Compiler.Objects
{
    /// <summary>
    /// A name pattern, as a value matches it: the value, evaluated once, matches what the
    /// type matches, or anything if there is no type, and if it matched, the target is set
    /// to it, as the type has it. The target is anything that can be set. The condition is
    /// the match's, once the target is set.
    /// </summary>
    internal sealed class Binding(CompilerObject target, CompilerObject? type, CompilerObject always)
        : CompilerObject
        , ICTPattern
    {
        public IResult Match(CompilerObject value)
        {
            if (Scope.TS.TypeOf(value).When(out var valueType).Error(out var error))
                return Result.Error(error);

            var matched = new Local(valueType!, value);

            if ((type is null ? Result.Ok(always) : Scope.Matching.Match(matched, type)).When(out var condition).Error(out error))
                return Result.Error(error);

            if (Scope.TS.TypeOf(condition!).When(out var conditionType).Error(out error))
                return Result.Error(error);

            var result = new Local(conditionType!, condition);

            if ((type is null ? Result.Ok(matched) : Scope.CG.Cast(matched, type)).When(out var narrowed).Error(out error))
                return Result.Error(error);

            if (Scope.CG.Set(target, narrowed!).When(out var set).Error(out error))
                return Result.Error(error);

            if (Scope.CG.Declare(matched).When(out var declareMatched).Error(out error))
                return Result.Error(error);

            if (Scope.CG.Declare(result).When(out var declareResult).Error(out error))
                return Result.Error(error);

            return Result.Ok(new BlockExpression(new Block([declareMatched!, declareResult!, new If(result, set!, null)]), result));
        }
    }
}
