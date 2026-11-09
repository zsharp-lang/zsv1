namespace ZSharp.LIR
{
    public sealed class Global(Type type) : ModuleMember
    {
        public Type Type { get; set; } = type;

        /// <summary>
        /// The value the global has before it is first read or written, if it is given
        /// one: evaluated once, and a module's initializers in the order its globals are.
        /// A target runs it as it initializes what it loads.
        /// </summary>
        public Expression? Initializer { get; set; }
    }
}
