using CommonZ.Utils;

namespace ZSharp.IR
{
    internal sealed class KwArgsCollection(Signature signature)
        : Collection<Parameter>
    {
        public Signature Signature { get; } = signature;

        public override void OnAdd(Parameter item)
        {
            AssertUnOwned(item);

            item.Signature = Signature;
            item.Index = Signature.TotalNumArgs + Count;
        }

        public override void OnRemoveAt(int index)
        {
            OnRemove(this[index]);
        }

        public override void OnRemove(Parameter item)
        {
            for (int i = item.Index + 1 - Signature.TotalNumArgs; i < Count; i++)
                this[i].Index--;

            item.Signature = null;
            item.Index = -1;
        }

        private static void AssertUnOwned(Parameter item)
        {
            if (item.Signature is not null)
                throw new InvalidOperationException();
        }
    }
}
