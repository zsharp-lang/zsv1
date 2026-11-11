using CommonZ;
using ZSharp.Compiler;

namespace ZSharp.CLI
{
    /// <summary>
    /// The CLI's diagnostics, which print to standard error so a program's own output stays
    /// as it is.
    /// </summary>
    internal static class Tracing
    {
        /// <summary>
        /// Prints each reification a meta-compiler asks for, with what it produced. Reifying
        /// code that was reified before shows up as its own entry, so each level can be seen.
        /// </summary>
        public static void TraceReification(HIRReifier reifier)
        {
            var reifyStatement = reifier.ReifyStatement;
            var reifyExpression = reifier.ReifyExpression;
            var depth = 0;
            var count = 0;

            reifier.ReifyStatement = statement => Trace(statement, node => reifyStatement(node));
            reifier.ReifyExpression = expression => Trace(expression, node => reifyExpression(node));

            IResult<HIR.Code.Expression, Error> Trace<T>(T node, Func<T, IResult<HIR.Code.Expression, Error>> reify)
            {
                if (depth++ > 0)
                    try { return reify(node); } finally { depth--; }

                var number = ++count;

                try
                {
                    var result = reify(node);

                    Console.Error.WriteLine($"reification {number}, of:" + Environment.NewLine + Diagnostics.HIRPrinter.Print(node));
                    Console.Error.WriteLine(result.When(out var reified).Error(out var error)
                        ? $"failed: {error}" + Environment.NewLine
                        : "produced:" + Environment.NewLine + Diagnostics.HIRPrinter.Print(reified) + Environment.NewLine);

                    return result;
                }
                finally
                {
                    depth--;
                }
            }
        }
    }
}
