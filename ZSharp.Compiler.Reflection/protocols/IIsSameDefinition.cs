namespace ZSharp.Compiler
{

    // this needs to be moved to a separate Reflection.Direct dispatcher so that
    // it can actually pass the Compiler into the method.
    public interface IIsSameDefinition
    {
        public bool IsSameDefinition(CompilerObject other);
    }
}
