namespace ZSharp.Importer.ILLoader
{
    /// <summary>
    /// A type that is a CLR type. CLR types only compare assignability with each other.
    /// </summary>
    internal interface IILType
    {
        public Type GetILType();
    }

    /// <summary>
    /// A class defined in Z# that is still being loaded, known by its HIR node.
    /// </summary>
    internal interface IDefinedClass
    {
        public Package.DotNet.Class Node { get; }
    }
}
