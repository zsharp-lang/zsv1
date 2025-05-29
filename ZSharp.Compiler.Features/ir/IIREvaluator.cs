using ZSharp.Objects;

namespace ZSharp.Compiler
{
    public interface IIREvaluator
    {
        /// <summary>
        /// Evaluate the given IR code and return the result object.
        /// 
        /// The return value depends on the properties of the IR code.
        /// <list type="table">
        ///     <listheader>
        ///         <term>Property</term>
        ///         <term>Result</term>
        ///     </listheader>
        ///     <item>
        ///         <description><see cref="IRCode.IsVoid"/></description>
        ///         <description><see langword="null"></see>.</description>
        ///     </item>
        ///     <item>
        ///         <description><see cref="IRCode.IsValue"/></description>
        ///         <description>A single <see cref="CompilerObject"/>.</description>
        ///     </item>
        ///     <item>
        ///         <description><see cref="IRCode.IsArray"/></description>
        ///         <description>An <see cref="UnpackedArray"/> object.</description>
        ///     </item>
        /// </list>
        /// </summary>
        /// <param name="code">The code to evaluate.</param>
        /// <returns>The result of evaluating the code.</returns>
        public CompilerObject? EvaluateCT(IRCode code);
    }
}
