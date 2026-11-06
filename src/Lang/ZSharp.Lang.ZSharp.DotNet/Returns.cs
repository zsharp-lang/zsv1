using CommonZ;

using Code = ZSharp.HIR.Code;

namespace ZSharp.Lang.DotNet
{
    /// <summary>
    /// Checks a body's returns against whether it returns a value. It sees only the
    /// statements of <c>ZSharp.HIR.Code</c>, so one added by an extension is treated
    /// as not returning.
    /// </summary>
    internal static class Returns
    {
        public static Error? Check(Code.Block body, bool returnsValue)
        {
            if (Mismatch(body, returnsValue) is { } error)
                return error;

            return returnsValue && !AlwaysReturns(body)
                ? new ErrorMessage("A body does not return a value on every path.")
                : null;
        }

        private static Error? Mismatch(Code.Statement statement, bool returnsValue)
            => statement switch
            {
                Code.Return { Value: null } when returnsValue
                    => new ErrorMessage("A return needs a value here."),
                Code.Return { Value: not null } when !returnsValue
                    => new ErrorMessage("A return has a value, but nothing is returned here."),
                Code.Block block
                    => block.Statements.Select(item => Mismatch(item, returnsValue)).FirstOrDefault(error => error is not null),
                Code.IfStatement @if
                    => Mismatch(@if.Then, returnsValue)
                    ?? (@if.Else is null ? null : Mismatch(@if.Else, returnsValue)),
                _ => null
            };

        private static bool AlwaysReturns(Code.Statement statement)
            => statement switch
            {
                Code.Return => true,
                Code.Block block => block.Statements.Any(AlwaysReturns),
                Code.IfStatement { Else: not null } @if => AlwaysReturns(@if.Then) && AlwaysReturns(@if.Else),
                _ => false
            };
    }
}
