namespace ZSharp.Runtime.NET
{
    public static class Utils
    {
        public static IL.MethodInfo GetMethod<T>(T t)
            where T : Delegate
            => t.Method;
    }
}
