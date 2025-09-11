namespace ZSharp.Interpreter
{
    public static class CTServices
    {
        public static CompilerObject InfoOf(object @object)
        {
            if (@object is CompilerObject co)
                return co;

            throw new NotImplementedException();
        }

        public static T? InfoOf<T>(object @object)
            where T : class, CompilerObject
        {
            return InfoOf(@object) as T;
        }
    }
}
