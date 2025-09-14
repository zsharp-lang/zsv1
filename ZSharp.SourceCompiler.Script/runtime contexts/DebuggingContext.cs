using System.Diagnostics.SymbolStore;
using ZSharp.IR.VM;
using ZSharp.Platform.Runtime.Loaders;

namespace ZSharp.SourceCompiler.Script
{
    internal sealed class DebuggingContext(ISymbolDocumentWriter document)
        : IDebuggableContext
    {
        ISymbolDocumentWriter IDebuggableContext.Document { get; } = document;
        private readonly Dictionary<Instruction, SourceLocation> sequencePoints = [];

        bool IDebuggableContext.TryGetSequencePoint(Instruction instruction, out SourceLocation location)
            => sequencePoints.TryGetValue(instruction, out location);

        public void AddSequencePoint(Instruction instruction, SourceLocation location)
            => sequencePoints[instruction] = location;
    }
}
