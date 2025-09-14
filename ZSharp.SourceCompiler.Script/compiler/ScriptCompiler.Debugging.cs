using CommonZ.Utils;
using System.Diagnostics.CodeAnalysis;

namespace ZSharp.SourceCompiler.Script
{
    partial class ScriptCompiler
    {
        internal string DocumentPath { get; init; }

        internal DebuggingContext? DebugContext { get; set; } = null!;

        [MemberNotNullWhen(true, nameof(DebugContext), nameof(CurrentEvaluationContext))]
        internal bool IsDebuggingEnabled => DebugContext is not null;

        private Platform.Runtime.IEvaluationContext? CurrentEvaluationContext { get; set; }

        private ContextManager EvaluationContext()
        {
            if (!Interpreter.Runtime.DebugEnabled) return new(() => { });
            CurrentEvaluationContext = Interpreter.Runtime.EvaluationContextFactory.CreateEvaluationContext();
            var document = CurrentEvaluationContext.Module.DefineDocument(DocumentPath);
            DebugContext = new(document);
            document.SetSource(File.ReadAllBytes(DocumentPath));
            return new(() => DebugContext = null);
        }
    }
}
