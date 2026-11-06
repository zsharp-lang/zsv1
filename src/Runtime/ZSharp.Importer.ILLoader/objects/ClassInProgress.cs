using ZSharp.Platform.Runtime;

namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A class while it is being loaded, which is how it refers to itself.
    /// </summary>
    internal sealed class ClassInProgress(ClassDefinition definition)
        : CompilerObject
        , ICompileIRType
    {
        public IResult<LIR.Type, Error> CompileIRType(object? target)
            => target is Runtime
                ? Result<LIR.Type>.Ok(definition)
                : Result<LIR.Type>.Error($"Class {definition.Name} can only be compiled for the CLR runtime.");
    }
}
