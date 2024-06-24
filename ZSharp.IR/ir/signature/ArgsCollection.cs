using CommonZ.Utils;

namespace ZSharp.IR
{
    internal sealed class ArgsCollection(Signature signature)
        : Collection<Parameter>
    {
        public Signature Signature { get; } = signature;

        public override void OnAdd(Parameter item)
        {
            AssertUnOwned(item);

            item.Signature = Signature;
            item.Index = Count;
        }

        public override void OnInsert(int index, Parameter item)
        {
            AssertUnOwned(item);

            item.Signature = Signature;
            item.Index = index;

            for (int i = index; i < Count; i++)
            {
                this[i].Index++;
            }
        }

        public override void OnRemove(Parameter item)
        {
            item.Signature = null;

            for (int i = item.Index; i < Count; i++)
            {
                this[i].Index--;
            }
            
            item.Index = -1;
        }

        public override void OnRemoveAt(int index)
        {
            OnRemove(this[index]);
        }

        private static void AssertUnOwned(Parameter item)
        {
            if (item.Signature is not null)
                throw new InvalidOperationException();
        }
    }
}
