using ZSharp.Compiler;

namespace ZSharp.SourceCompiler.Script.Objects
{
    internal sealed class ExpressionWrapper(
        DebuggingContext debugContext, 
        Text.Span span,
        CompilerObject inner
    )
        : CompilerObject
        , IProxy
        , ICompileIRCode
    {
        private readonly DebuggingContext debugContext = debugContext;
        private readonly Text.Span span = span;
        private readonly CompilerObject inner = inner;

        R IProxy.Apply<R>(Func<CompilerObject, R> fn)
            => fn(inner);

        Result<IRCode> ICompileIRCode.CompileIRCode(Compiler.Compiler compiler, object? target)
        {
            if (target is not Platform.Runtime.Runtime runtime) 
                return compiler.IR.CompileCode(inner, target);

            var innerCodeResult = compiler.IR.CompileCode(inner, target);

            if (
                innerCodeResult
                .When(out var innerCode)
                .Error(out var error)
            ) return Result<IRCode>.Error(error);

            debugContext.AddSequencePoint(
                innerCode!.Instructions[0],
                new()
                {
                    StartLine = span.Start.Line,
                    StartColumn = span.Start.Column,
                    EndLine = span.End.Line,
                    EndColumn = span.End.Column
                }
            );

            return innerCodeResult;
        }
    }
}
