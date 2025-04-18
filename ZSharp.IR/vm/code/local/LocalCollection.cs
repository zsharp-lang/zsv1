﻿using CommonZ.Utils;

namespace ZSharp.IR.VM
{
    internal sealed class LocalCollection(IRObject owner) : Collection<Local>
    {
        private readonly IRObject owner = owner;

        public override void OnAdd(Local item)
        {
            base.OnAdd(item);

            AssertUnOwned(item);

            item.Owner = owner;
            item.Index = Count;
        }

        public override void OnInsert(int index, Local item)
        {
            base.OnInsert(index, item);

            AssertOwned(item);

            item.Index = index;
            item.Owner = owner;

            for (int i = index + 1; i < Count; i++)
                this[i].Index++;
        }

        public override void OnRemove(Local item)
        {
            base.OnRemove(item);

            OnRemoveAt(item.Index);
        }

        public override void OnRemoveAt(int index)
        {
            base.OnRemoveAt(index);

            Local item = this[index];

            AssertOwned(item);

            item.Owner = null;

            for (int i = index + 1; i < Count; i++)
                this[i].Index--;

            item.Index = -1;
        }

        private void AssertOwned(Local item) {
            if (item.Owner != owner)
                throw new InvalidOperationException();
        }

        private void AssertUnOwned(Local item)
        {
            if (item.Owner != null)
                throw new InvalidOperationException();
        }
    }
}
