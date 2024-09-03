using ZSharp.IR;

namespace ZSharp.VM
{
    public sealed class ZSInstance(
        int instanceFieldCount,
        int prototypeFieldCount,
        ZSClass type
    ) : ZSStruct(instanceFieldCount + prototypeFieldCount, type)
    {
        public int InstanceFieldCount { get; internal set; } = instanceFieldCount;

        public int PrototypeFieldCount { get; internal set; } = prototypeFieldCount;

        public ZSClass Class { get; internal set; } = type;

        public ZSObject GetField(Field field)
            => (field.Attributes & FieldAttributes.BindingMask) switch
            {
                FieldAttributes.Instance => GetField(field.Index),
                FieldAttributes.Prototype => GetField(InstanceFieldCount + field.Index),
                _ => Class.GetField(field)
            };

        public void SetField(Field field, ZSObject value)
        {
            switch (field.Attributes & FieldAttributes.BindingMask)
            {
                case FieldAttributes.Instance: SetField(field.Index, value); break;
                case FieldAttributes.Prototype: SetField(InstanceFieldCount + field.Index, value); break;
                default: Class.SetField(field, value); break;
            }
        }

        public static ZSInstance CreateFrom(ZSClass @class)
        {
            int instanceFieldCount = 0, prototypeFieldCount = 0;

            if (@class.IR.HasFields)
                foreach (Field field in @class.IR.Fields)
                    _ = (field.Attributes & FieldAttributes.BindingMask) switch
                    {
                        FieldAttributes.Instance => instanceFieldCount++,
                        FieldAttributes.Prototype => prototypeFieldCount++,
                        _ => 0
                    };

            return new(instanceFieldCount, prototypeFieldCount, @class);
        }
    }
}
