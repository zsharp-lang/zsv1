namespace ZSharp.Importer.ILLoader.Objects
{
    partial class GenericTypeInstance
        : IReferenceType
        , IILType
    {
        Type IILType.GetILType()
            => IL;
    }
}
