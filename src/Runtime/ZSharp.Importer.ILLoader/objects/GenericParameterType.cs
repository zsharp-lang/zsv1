using ZSharp.LIR.CLR;

namespace ZSharp.Importer.ILLoader.Objects
{
    internal interface IGenericParameter
    {
        public Package.DotNet.GenericParameter Node { get; }
    }

    /// <summary>
    /// A type parameter of a generic class or method defined in Z#, as its definition is
    /// lowered for each target. A value of it only goes where the same parameter is
    /// expected.
    /// </summary>
    internal sealed class GenericParameterType(Package.DotNet.GenericParameter node, Func<object?, IResult<LIR.Type, Error>> lowered)
        : CompilerObject
        , ICompileIRType
        , IAssignableTo
        , IGenericParameter
    {
        public Package.DotNet.GenericParameter Node { get; } = node;

        public IResult<LIR.Type, Error> CompileIRType(object? target)
            => lowered(target);

        public bool IsAssignableTo(CompilerObject target)
            => target.Is<IGenericParameter>(out var other) && other.Node == Node;
    }
}
