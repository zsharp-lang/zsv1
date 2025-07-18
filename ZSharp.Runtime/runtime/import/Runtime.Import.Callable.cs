namespace ZSharp.Runtime
{
    partial class Runtime
    {
        public IL.MethodBase ImportCallable(IR.ICallable callable)
            => callable switch
            {
                IR.ConstructorReference target => ImportConstructorReference(target),
                IR.Method target => ImportMethod(target),
                IR.MethodReference target => ImportMethodReference(target),
                IR.Function target => ImportFunction(target),
                IR.GenericFunctionInstance target => ImportConstructedFunction(target),
                _ => throw new ArgumentException(
                    $"Invalid callable type: {callable.GetType()}",
                    nameof(callable)
                )
            };
    }
}
