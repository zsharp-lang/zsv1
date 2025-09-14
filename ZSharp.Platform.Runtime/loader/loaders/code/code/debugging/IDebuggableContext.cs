namespace ZSharp.Platform.Runtime.Loaders
{
    public interface IDebuggableContext
        : IContext
    {
        public System.Diagnostics.SymbolStore.ISymbolDocumentWriter Document { get; }

        public bool TryGetSequencePoint(IR.VM.Instruction instruction, out SourceLocation location);
    }
}
