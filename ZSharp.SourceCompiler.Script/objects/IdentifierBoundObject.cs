using ZSharp.Compiler;

namespace ZSharp.SourceCompiler.Script.Objects
{
    internal sealed class IdentifierBoundObject(
        ScriptCompiler compiler, 
        AST.IdentifierExpression identifier,
        CompilerObject inner
    )
        : CompilerObject
        , IProxy
        , ICompileIRCode
    {
        private readonly ScriptCompiler compiler = compiler;
        private readonly AST.IdentifierExpression identifier = identifier;
        private readonly CompilerObject inner = inner;

        R IProxy.Apply<R>(Func<CompilerObject, R> fn)
            => fn(inner);

        Result<IRCode> ICompileIRCode.CompileIRCode(Compiler.Compiler compiler, object? target)
        {
            if (target is not Platform.Runtime.Runtime runtime || !this.compiler.IsDebuggingEnabled) 
                return compiler.IR.CompileCode(inner, target);

            var innerCodeResult = compiler.IR.CompileCode(inner, target);

            if (
                innerCodeResult
                .When(out var innerCode)
                .Error(out var error)
            ) return Result<IRCode>.Error(error);

            var span = identifier.TokenInfo.Identifier.Span;
            this.compiler.DebugContext.AddSequencePoint(
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
