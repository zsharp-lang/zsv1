using CommonZ.Utils;

namespace ZSharp.VM
{
    public interface IScope
    {
        public Mapping<string, ZSObject> Members { get; }
    }
}
