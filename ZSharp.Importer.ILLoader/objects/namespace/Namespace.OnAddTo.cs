namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Namespace
        : IOnAddTo<Namespace>
    {
        OnAddResult IOnAddTo<Namespace>.OnAddTo(Namespace @object)
        {
            if (HasParent) return OnAddResult.Error;

            Parent = @object;

            return OnAddResult.None;
        }
    }
}
