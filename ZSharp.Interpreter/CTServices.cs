namespace ZSharp.Interpreter
{
    public static class CTServices
    {
        public static CompilerObject InfoOf(object @object)
        {
            throw new NotImplementedException();
        }

        public static T? InfoOf<T>(object @object)
            where T : class, CompilerObject
        {
            return InfoOf(@object) as T;
        }
    }
}
