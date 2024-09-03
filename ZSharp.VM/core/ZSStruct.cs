namespace ZSharp.VM
{
    public abstract class ZSStruct(int fieldCount, ZSObject type) : ZSObject(type)
    {
        private ZSObject[] _fields = new ZSObject[fieldCount];

        public int ObjectSize => _fields.Length;

        internal void Resize(int fieldCount)
            => Array.Resize(ref _fields, fieldCount);

        public ZSObject GetField(int index)
            => _fields[index];

        public void SetField(int index, ZSObject value)
            => _fields[index] = value;
    }
}
