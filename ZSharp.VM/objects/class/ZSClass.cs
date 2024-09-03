using System.Diagnostics.CodeAnalysis;
using ZSharp.IR;

namespace ZSharp.VM
{
    public sealed class ZSClass(
            Class ir,
            int classFieldCount,
            int staticFieldCount,
            ZSObject type,
            bool hasVTable = false
        ) : ZSIRObject<Class>(ir, classFieldCount + staticFieldCount, type)
    {
        private VTable? vTable = hasVTable ? new() : null;

        internal VTable? VTable => vTable;

        [MemberNotNullWhen(true, nameof(vTable))]
        [MemberNotNullWhen(true, nameof(VTable))]
        public bool HasVTable => vTable is not null;

        public int ClassFieldCount { get; internal set; } = classFieldCount;

        public int StaticFieldCount { get; internal set; } = staticFieldCount;

        public ZSObject GetField(Field field)
            => (field.Attributes & FieldAttributes.BindingMask) switch
            {
                FieldAttributes.Static => GetField(ClassFieldCount + field.Index),
                FieldAttributes.Class => GetField(field.Index),
                _ => throw new UnboundFieldException(field)
            };

        public void SetField(Field field, ZSObject value)
        {
            switch (field.Attributes & FieldAttributes.BindingMask)
            {
                case FieldAttributes.Class: SetField(field.Index, value); break;
                case FieldAttributes.Static: SetField(ClassFieldCount + field.Index, value); break;
                default: throw new UnboundFieldException(field);
            }
        }

        public static ZSClass CreateFrom(Class ir, ZSObject type)
        {
            int classFieldCount = 0, staticFieldCount = 0;

            if (ir.HasFields)
                foreach (Field field in ir.Fields)
                    _ = (field.Attributes & FieldAttributes.BindingMask) switch
                    {
                        FieldAttributes.Static => staticFieldCount++,
                        FieldAttributes.Class => classFieldCount++,
                        _ => 0
                    };

            // todo: check if the class has a vtable and populate it

            return new(ir, classFieldCount, staticFieldCount, type);
        }
    }
}
