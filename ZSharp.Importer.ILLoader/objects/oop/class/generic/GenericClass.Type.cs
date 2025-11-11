namespace ZSharp.Importer.ILLoader.Objects
{
    partial class GenericClass
        : IReferenceType
        , IILType
    {
        Type IILType.GetILType()
            => IL;
    }
}
