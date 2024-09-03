using CommonZ.Utils;

using FieldOwner = ZSharp.IR.Class;

namespace ZSharp.IR
{
    internal sealed class FieldCollection(FieldOwner owner) : Collection<Field>
    {
        private readonly FieldOwner _owner = owner;

        private int _classFieldCount;
        private int _instanceFieldCount;
        private int _prototypeFieldCount;
        private int _staticFieldCount;

        public override void OnAdd(Field item)
        {
            base.OnAdd(item);

            AddField(item);
        }

        public override void OnInsert(int index, Field item)
        {
            base.OnInsert(index, item);

            AddField(item);

            for (int i = index + 1; i < Count; i++)
                if (this[i].Binding == item.Binding)
                    this[i].Index++;
        }

        public override void OnRemove(Field item)
        {
            base.OnRemove(item);

            RemoveFieldWithIndex(item, IndexOf(item));

            UpdateCountWithRemove(item.Attributes);
        }

        public override void OnRemoveAt(int index)
        {
            base.OnRemoveAt(index);

            RemoveFieldWithIndex(this[index], index);
        }

        internal void ChangeFieldBinding(Field field, FieldAttributes attributes)
        {
            attributes &= FieldAttributes.BindingMask;

            if (field.Binding == attributes)
                return;

            UpdateCountWithRemove(field.Attributes);

            field._attributes = (field.Attributes & ~FieldAttributes.BindingMask) | attributes;

            UpdateCountWithAdd(attributes);
        }

        private void AssertUnOwned(Field field)
        {
            if (field.Owner is not null)
                throw new InvalidOperationException("The field is already owned by a different object.");
        }

        private void AssertOwned(Field field)
        {
            if (field.Owner != _owner)
                throw new InvalidOperationException("The field is not owned by this collection.");
        }

        private void AddField(Field field)
        {
            AssertUnOwned(field);

            field.Owner = _owner;
            field.Index = UpdateCountWithAdd(field.Attributes);
        }

        private void RemoveFieldWithIndex(Field field, int index)
        {
            AssertOwned(field);

            field.Owner = null;

            for (int i = index + 1; i < Count; i++)
                if (this[i].Binding == field.Binding)
                    this[i].Index--;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns>The count before updating.</returns>
        private int UpdateCountWithAdd(FieldAttributes attributes)
            => (attributes & FieldAttributes.BindingMask) switch
            {
                FieldAttributes.Class => _classFieldCount++,
                FieldAttributes.Instance => _instanceFieldCount++,
                FieldAttributes.Prototype => _prototypeFieldCount++,
                FieldAttributes.Static => _staticFieldCount++,
                _ => 0
            };
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns>The count before updating.</returns>
        private int UpdateCountWithRemove(FieldAttributes attributes)
            => (attributes & FieldAttributes.BindingMask) switch
            {
                FieldAttributes.Class => _classFieldCount--,
                FieldAttributes.Instance => _instanceFieldCount--,
                FieldAttributes.Prototype => _prototypeFieldCount--,
                FieldAttributes.Static => _staticFieldCount--,
                _ => 0
            };
    }
}
