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

            Class @string;
            Class type;
            Class @void;
            Class @null;

            Class boolean;

            Class int32;

            Class float32;

            {
                module.Types.Add(type = new("Type"));
                module.Types.Add(@string = new("String"));
                module.Types.Add(@void = new("Void"));
                module.Types.Add(@null = new("Null"));

                module.Types.Add(boolean = new("Boolean"));

                module.Types.Add(int32 = new("Int32"));

                module.Types.Add(float32 = new("Float32"));
            }

            return new(module, new()
            {
                String = @string,
                Type = type,
                Void = @void,
                Null = @null,

                Boolean = boolean,

                Int32 = int32,

                Float32 = float32,
            });
        }
    }
}
