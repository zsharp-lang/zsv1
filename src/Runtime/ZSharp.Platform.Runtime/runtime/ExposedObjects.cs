namespace ZSharp.Platform.Runtime
{
    /// <summary>
    /// Objects made available to emitted code. A constant of a type IL has no
    /// instruction for is stored here and read back by index.
    /// </summary>
    public static class ExposedObjects
    {
        private static readonly List<object> objects = [];

        public static object Get(int index)
            => objects[index];

        internal static int Add(object value)
        {
            objects.Add(value);

            return objects.Count - 1;
        }
    }
}
