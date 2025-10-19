namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Constructor
        : IOnAddTo<CompilerObject>
    {
        public CompilerObject? Owner { get; private set; }

        OnAddResult IOnAddTo<CompilerObject>.OnAddTo(CompilerObject @object)
        {
            if (@object is null || Owner is not null && @object != Owner)
                return OnAddResult.Error;

            Owner = @object;
            return OnAddResult.None;
        }
    }
}
