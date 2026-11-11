using System.Collections.ObjectModel;

namespace ZSharp.LIR
{
    public sealed class ModuleCollection<T> : Collection<T>
        where T : ModuleMember
    {
        private readonly Module owner;

        internal ModuleCollection(Module owner)
        {
            this.owner = owner;
        }

        protected override void InsertItem(int index, T item)
        {
            Claim(item);
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, T item)
        {
            var previous = this[index];

            if (ReferenceEquals(previous, item))
                return;

            Claim(item);
            previous.Module = null;
            base.SetItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            this[index].Module = null;
            base.RemoveItem(index);
        }

        protected override void ClearItems()
        {
            foreach (var item in this)
                item.Module = null;

            base.ClearItems();
        }

        private void Claim(T item)
        {
            if (item.Module is not null)
                throw new InvalidOperationException(
                    $"{item} is already owned by {item.Module} and cannot be added to {owner}."
                );

            item.Module = owner;
        }
    }
}
