namespace CommonZ.Utils
{
    public class OwnedCollection<Owned, Owner> : Collection<Owned> 
        where Owned : class, IOwned<Owner>
        where Owner : class
    {
        private readonly Owner _owner;

        public OwnedCollection(Owner owner)
        {
            _owner = owner;
        }

        public override void OnAdd(Owned item)
        {
            BindItem(item);
        }

        public override void OnInsert(int index, Owned item)
        {
            BindItem(item);
        }

        public override void OnRemove(Owned item)
        {
            UnbindItem(item);
        }

        public override void OnRemoveAt(int index)
        {
            UnbindItem(this[index]);
        }

        private void BindItem(Owned item)
        {
            AssertUnboundItem(item);
            item.Owner = _owner;
        }

        private void UnbindItem(Owned item)
        {
            AssertBoundItem(item);
            item.Owner = null;
        }

        private void AssertUnboundItem(Owned item)
        {
            if (item.Owner != null && item.Owner != _owner)
            {
                throw new InvalidOperationException();
            }
        }

        private void AssertBoundItem(Owned item)
        {
            if (item.Owner != _owner)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
