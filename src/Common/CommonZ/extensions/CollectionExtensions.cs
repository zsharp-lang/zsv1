namespace CommonZ.Utils
{
    public static class CollectionExtensions
    {
        public static bool IsNullOrEmpty<T>(this Collection<T>? collection)
        {
            return collection is null || collection.Count == 0;
        }
    }
}
