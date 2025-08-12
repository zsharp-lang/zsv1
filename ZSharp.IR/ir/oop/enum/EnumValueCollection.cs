using CommonZ.Utils;

namespace ZSharp.IR
{
    internal sealed class EnumValueCollection(EnumClass owner)
        : Collection<EnumValue>
    {
        private readonly EnumClass enumClass = owner;

        public override void OnAdd(EnumValue item)
        {
            AssertUnonwed(item);
            SetOwner(item);
        }

        public override void OnInsert(int index, EnumValue item)
        {
            AssertUnonwed(item);
            SetOwner(item);
        }

        public override void OnRemove(EnumValue item)
        {
            AssertOwned(item);
            UnsetOwner(item);
        }

        public override void OnRemoveAt(int index)
        {
            OnRemove(this[index]);
        }

        private void SetOwner(EnumValue item)
        {
            item._enumClass = enumClass;
        }

        private void UnsetOwner(EnumValue item)
        {
            item._enumClass = null;
        }

        private void AssertUnonwed(EnumValue item)
        {
            if (item.Module is not null)
                throw new InvalidOperationException();
        }

        private void AssertOwned(EnumValue item)
        {
            if (item.Module is null)
                throw new InvalidOperationException();
        }
    }
}
