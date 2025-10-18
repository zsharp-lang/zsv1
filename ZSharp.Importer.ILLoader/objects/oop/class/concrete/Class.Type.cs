namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Class
        : IReferenceType
        , IILType
    {
        Type IILType.GetILType()
            => IL;
    }
}
