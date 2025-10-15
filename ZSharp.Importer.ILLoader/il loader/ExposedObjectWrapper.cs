namespace ZSharp.Importer.ILLoader
{
    public sealed class ExposedObjectWrapper(object? target, IL.MethodInfo method)
    {
        private readonly object? target = target;
        private readonly IL.MethodInfo method = method;

        public object? Invoke(object? args)
            => method.Invoke(target, [args]);
    }
}
