using ZSharp.Platform.Runtime;

namespace ZSharp.Importer.ILLoader.Objects
{
    internal interface IGenericParameter
    {
        public Package.DotNet.GenericParameter Node { get; }
    }

    /// <summary>
    /// A type parameter of a generic class being loaded. A value of it only goes where the
    /// same parameter is expected.
    /// </summary>
    internal sealed class GenericParameterType(Package.DotNet.GenericParameter node, ClassGenericParameter lowered)
        : CompilerObject
        , ICompileIRType
        , IAssignableTo
        , IGenericParameter
    {
        public Package.DotNet.GenericParameter Node { get; } = node;

        public IResult<LIR.Type, Error> CompileIRType(object? target)
            => Result<LIR.Type>.Ok(lowered);

        public bool IsAssignableTo(CompilerObject target)
            => target.Is<IGenericParameter>(out var other) && other.Node == Node;
    }
}
