using ZSharp.HIR;

namespace ZSharp.SourceCompiler.Script
{
    public sealed class Document
    {
        public HIR.Code.Block Content { get; set; } = new();

        public Reference? Export(string name)
        {
            throw new NotImplementedException();
        }

        public void Export(string name, Reference reference)
        {
            throw new NotImplementedException();
        }
    }
}
