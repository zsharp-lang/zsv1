namespace ZSharp.Runtime
{
    partial class Runtime
    {
        private IL.MethodInfo LoadFunction(IR.Function function)
        {
            if (function.Module is not null)
                throw new InvalidOperationException(
                    $"Cannot directly load function {function.Name} owned by module {function.Module.Name}"
                );

            return Loader.LoadStandaloneFunction(function);
        }
    }
}
