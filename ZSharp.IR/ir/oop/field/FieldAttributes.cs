namespace ZSharp.IR
{
    [Flags]
    public enum FieldAttributes
    {
        None = Instance,

        /// <summary>
        /// Right bit (0) represents scope: 0 - inheritance, 1 - this.
        /// Left bit (1) represents binding: 0 - class, 1 - instance.
        /// </summary>
        BindingMask = 0b11,

        Class = 0b00,
        Static = 0b01,
        Instance = 0b10,
        Prototype = 0b11,

        ReadOnly = 0b100,
    }
}