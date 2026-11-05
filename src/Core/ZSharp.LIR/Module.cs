namespace ZSharp.LIR
{
    /// <summary>
    /// The root of every graph. Members may directly reference entities owned
    /// by a different module.
    /// </summary>
    public sealed class Module : Definition
    {
        public ModuleCollection<Function> Functions { get; }

        public ModuleCollection<Global> Globals { get; }

        public ModuleCollection<Structure> Structures { get; }

        public Module(string? name = null)
        {
            Name = name;
            Functions = new(this);
            Globals = new(this);
            Structures = new(this);
        }
    }
}
