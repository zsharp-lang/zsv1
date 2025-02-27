namespace ZSharp.Objects
{
    internal static class RuntimeHelpers
    {
        public static T GetUninitializedObject<T>()
            => (T)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(
                typeof(T)
            );
    }
}
