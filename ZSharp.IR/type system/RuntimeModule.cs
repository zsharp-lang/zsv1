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

            ConstructedClass @object;
            ConstructedClass @string;
            ConstructedClass type;
            ConstructedClass @void;
            ConstructedClass @null;

            ConstructedClass boolean;

            ConstructedClass int32;

            ConstructedClass float32;

            {
                module.Types.Add((type = new(new("Type"))).Class);
                module.Types.Add((@object = new(new("Object"))).Class);
                module.Types.Add((@string = new(new("String"))).Class);
                module.Types.Add((@void = new(new("Void"))).Class);
                module.Types.Add((@null = new(new("Null"))).Class);

                module.Types.Add((boolean = new(new("Boolean"))).Class);

                module.Types.Add((int32 = new(new("Int32"))).Class);

                module.Types.Add((float32 = new(new("Float32"))).Class);
            }

            return new(module, new()
            {
                Object = @object,
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
