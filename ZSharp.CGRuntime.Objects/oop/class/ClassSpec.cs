using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    /// <summary>
    /// Holds information about a class before constructing an instance
    /// of the metaclass. An instance of this class is passed to the metaclass
    /// constructor.
    /// </summary>
    public sealed class ClassSpec
        : CGObject
    {
        public string? Name { get; set; }

        public required CGObject MetaClass { get; set; }

        public List<CGObject>? Bases { get; set; }

        public List<CGObject>? Content { get; set; }
    }
}
