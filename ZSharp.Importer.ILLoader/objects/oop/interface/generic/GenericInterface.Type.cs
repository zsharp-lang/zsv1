namespace ZSharp.Importer.ILLoader.Objects
{
    partial class GenericInterface
        : IReferenceType
        , IILType
    {
        Type IILType.GetILType()
            => IL;
    }
}
