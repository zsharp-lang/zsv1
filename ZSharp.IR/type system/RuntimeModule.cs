namespace ZSharp.IR
{
    public sealed class RuntimeModule(Module module, TypeSystem typeSystem)
    {
        private readonly Module _module = module;
        private readonly TypeSystem _typeSystem = typeSystem;

        public Module Module => _module;

        public TypeSystem TypeSystem => _typeSystem;

        public static RuntimeModule Standard { get; } = CreateStandardRuntimeModule();

        private static RuntimeModule CreateStandardRuntimeModule()
        {
            Module module = new("Runtime");

            Class any;
            Class @string;
            Class @void;

            {
                module.Types.Add(any = new("Any"));
                module.Types.Add(@string = new("String"));
                module.Types.Add(@void = new("Void"));
            }

            return new(module, new()
            {
                Any = any,
                String = @string,
                Void = @void
            });
        }
    }
}
