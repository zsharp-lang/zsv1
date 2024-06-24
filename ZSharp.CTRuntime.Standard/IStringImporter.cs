using ZSharp.VM;

namespace ZSharp.IR.Standard
{
    public interface IStringImporter
    {
        public ZSObject Import(string source);
    }
}
