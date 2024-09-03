namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        /// <summary>
        /// Returns whether the given object is the object representing `true`.
        /// </summary>
        /// <param name="object">The object to test.</param>
        /// <returns><see langword="true"/> if the object is `true`, <see langword="false"/> otherwise.</returns>
        public bool IsTrue(ZSObject @object)
            => ReferenceEquals(@object, True);

        /// <summary>
        /// Returns whether the given object is the object representing `false`.
        /// </summary>
        /// <param name="object">The object to test.</param>
        /// <returns><see langword="true"/> if the object is `false`, <see langword="false"/> otherwise.</returns>
        public bool IsFalse(ZSObject @object)
            => ReferenceEquals(@object, False);
    }
}
