namespace CommonZ.Utils
{
    public class MaybeOwnedCollection<Owned, Owner> : Collection<Owned>
        where Owned : class
        where Owner : class
    {
        private readonly Owner _owner;

        public MaybeOwnedCollection(Owner owner)
        {
            _owner = owner;
        }

        public override void OnAdd(Owned item)
        {
            if (item is IOwned<Owner> owned)
                BindItem(owned);
        }

        public override void OnInsert(int index, Owned item)
        {
            if (item is IOwned<Owner> owned)
                BindItem(owned);
        }

        public override void OnRemove(Owned item)
        {
            if (item is IOwned<Owner> owned)
                UnbindItem(owned);
        }

        public override void OnRemoveAt(int index)
        {
            if (this[index] is IOwned<Owner> owned)
                UnbindItem(owned);
        }

        private void BindItem(IOwned<Owner> item)
        {
            AssertUnboundItem(item);
            item.Owner = _owner;
        }

        private void UnbindItem(IOwned<Owner> item)
        {
            AssertBoundItem(item);
            item.Owner = null;
        }

        private void AssertUnboundItem(IOwned<Owner> item)
        {
            if (item.Owner != null && item.Owner != _owner)
            {
                throw new InvalidOperationException();
            }
        }

        private void AssertBoundItem(IOwned<Owner> item)
        {
            if (item.Owner != _owner)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
