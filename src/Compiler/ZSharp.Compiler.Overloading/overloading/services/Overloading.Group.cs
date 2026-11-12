namespace ZSharp.Compiler
{
    /// <summary>
    /// The one object several candidates stand for together, as the members of one name
    /// do. A single candidate stands for itself.
    /// </summary>
    public delegate IResult Group(CompilerObject[] candidates);

    partial class Overloading
    {
        public Group Group { get; set; } = Dispatcher.Group;
    }
}
