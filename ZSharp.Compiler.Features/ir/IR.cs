using ZSharp.Objects;

namespace ZSharp.Compiler
{
    public sealed partial class IRCompiler(Compiler compiler) : Feature(compiler)
    {
        public ZSharp.IR.RuntimeModule RuntimeModule
            => Compiler.RuntimeModule;

        public IIREvaluator? Evaluator { get; set; }

        public CompilerObject? EvaluateCO(IRCode code)
            => Evaluator is not null
            ? Evaluator.EvaluateCT(code)
            : throw new InvalidOperationException(
                "No IR evaluator was assigned."
            );
    }
}
