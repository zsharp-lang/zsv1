using CommonZ.Utils;

namespace ZSharp.IR.VM
{
    internal sealed class InstructionCollection : Collection<Instruction>
    {
        public InstructionCollection() : base() { }

        public InstructionCollection(IEnumerable<Instruction> instructions)
            : base(instructions)
        {
            for (int i = 1; i < Count - 1; ++i)
            {
                Instruction instruction = this[i];
                instruction.Index = i;
                instruction.Previous = this[i - 1];
                instruction.Next = this[i + 1];
            }

            if (Count > 0)
            {
                this[0].Index = 0;
                this[0].Next = this[1];
            }

            if (Count > 1)
            {
                this[Count - 1].Index = Count - 1;
                this[Count - 1].Previous = this[Count - 2];
            }
        }

        public override void OnAdd(Instruction item)
        {
            item.Index = Count;
            if (Count == 0) return;

            var prev = this[Count - 1];

            prev.Next = item;
            item.Previous = prev;
        }

        public override void OnInsert(int index, Instruction item)
        {
            item.Index = index;

            if (Count == 0) return;

            var last = this[index];

            item.Next = last;
            item.Previous = last.Previous;
            last.Previous = item;
        }

        public override void OnRemove(Instruction item)
        {
            if (item.Previous is not null)
                item.Previous.Next = item.Next;
            if (item.Next is not null)
                item.Next.Previous = item.Previous;

            for (int i = item.Index + 1; i < Count; ++i)
                --this[i].Index;

            item.Next = item.Previous = null;
            item.Index = -1;
        }

        public override void OnRemoveAt(int index)
        {
            OnRemove(this[index]);
        }
    }
}
