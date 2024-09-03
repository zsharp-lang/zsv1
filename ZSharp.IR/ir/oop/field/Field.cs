﻿namespace ZSharp.IR
{
    public sealed class Field
    {
        internal FieldAttributes _attributes = FieldAttributes.None;

        public string Name { get; set; }

        public FieldAttributes Attributes
        {
            get => _attributes;
            set
            {
                _attributes = value;
                (Owner?.Fields as FieldCollection)!.ChangeFieldBinding(this, value);
            }
        }

        public FieldAttributes Binding
        {
            get => Attributes & FieldAttributes.BindingMask;
            private set => Attributes = (Attributes & ~FieldAttributes.BindingMask) | value;
        }

        public bool IsClass
        {
            get => Binding == FieldAttributes.Class;
            set => _ = value
                ? Binding = FieldAttributes.Class
                : Binding = FieldAttributes.None;
        }

        public bool IsInstance
        {
            get => Binding == FieldAttributes.Instance;
            set => _ = value
                ? Binding = FieldAttributes.Instance
                : Binding = FieldAttributes.None;
        }

        public bool IsPrototype
        {
            get => Binding == FieldAttributes.Prototype;
            set => _ = value
                ? Binding = FieldAttributes.Prototype
                : Binding = FieldAttributes.None;
        }

        public bool IsStatic
        {
            get => Binding == FieldAttributes.Static;
            set => _ = value
                ? Binding = FieldAttributes.Static
                : Binding = FieldAttributes.None;
        }

        /// <summary>
        /// The index of the field relative to other fields with the same binding.
        /// </summary>
        public int Index { get; internal set; }

        public Class? Owner { get; internal set; }

        public IType Type { get; set; }

        public VM.Instruction[]? Initializer { get; set; }
    }
}
