namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        public ZSObject False { get; } = new ZSValue(TypeSystem.Boolean);

        public ZSObject True { get; } = new ZSValue(TypeSystem.Boolean);

        /// <summary>
        /// Returns whether the given object is the object representing `true`.
        /// </summary>
        /// <param name="object">The object to test.</param>
        /// <returns><see cref="true"/> if the object is `true`, <see cref="false"/> otherwise.</returns>
        public bool IsTrue(ZSObject @object)
            => ReferenceEquals(@object, True);

        /// <summary>
        /// Returns whether the given object is the object representing `false`.
        /// </summary>
        /// <param name="object">The object to test.</param>
        /// <returns><see cref="true"/> if the object is `false`, <see cref="false"/> otherwise.</returns>
        public bool IsFalse(ZSObject @object)
            => ReferenceEquals(@object, False);
    }
}
