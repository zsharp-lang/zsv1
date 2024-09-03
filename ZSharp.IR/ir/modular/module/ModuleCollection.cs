using CommonZ.Utils;

namespace ZSharp.IR
{
    internal class ModuleCollection<T>(Module owner) : Collection<T>
        where T : ModuleMember
    {
        public Module Module { get; } = owner;

        public override void OnAdd(T item)
        {
            AssertUnOwned(item);

            item.Owner = Module;
        }

        public override void OnInsert(int index, T item)
        {
            AssertUnOwned(item);

            item.Owner = Module;
        }

        public override void OnRemove(T item)
        {
            item.Owner = null;
        }

        public override void OnRemoveAt(int index)
        {
            this[index].Owner = null;
        }

        private void AssertUnOwned(T item)
        {
            if (item.Module is not null)
                throw new InvalidOperationException(
                    $"{item} is already owned by {item.Module} and cannot be assigned to {Module}"
                );
        }
    }
}
