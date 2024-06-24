using CommonZ.Utils;

namespace ZSharp.IR
{
    internal class ModuleCollection<T>(Module owner) : Collection<T>
        where T : IRObject
    {
        public Module Module { get; } = owner;

        public override void OnAdd(T item)
        {
            AssertUnOwned(item);

            item.Module = Module;
        }

        public override void OnInsert(int index, T item)
        {
            AssertUnOwned(item);

            item.Module = Module;
        }

        public override void OnRemove(T item)
        {
            item.Module = null;
        }

        public override void OnRemoveAt(int index)
        {
            this[index].Module = null;
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
