namespace ZSharp.IR
{
    internal sealed class GlobalCollection(Module owner) : ModuleCollection<Global>(owner)
    {
        public override void OnAdd(Global item)
        {
            base.OnAdd(item);

            item.Index = Count;
        }

        public override void OnInsert(int index, Global item)
        {
            base.OnInsert(index, item);

            for (int i = index; i < Count; i++)
                this[i].Index = i;
        }

        public override void OnRemove(Global item)
        {
            base.OnRemove(item);

            for (int i = item.Index; i < Count; i++)
                this[i].Index = i;
        }

        public override void OnRemoveAt(int index)
        {
            OnRemove(this[index]);
        }
    }
}
