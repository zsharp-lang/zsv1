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

            ClassReference @object;
            ClassReference @string;
            ClassReference type;
            ClassReference @void;
            ClassReference @null;

            ClassReference boolean;

            ClassReference int32;

            ClassReference float32;

            Class array;
            Class reference;
            Class pointer;

            {
                module.Types.Add((type = new(new("Type"))).Definition);
                module.Types.Add((@object = new(new("Object"))).Definition);
                module.Types.Add((@string = new(new("String"))).Definition);
                module.Types.Add((@void = new(new("Void"))).Definition);
                module.Types.Add((@null = new(new("Null"))).Definition);

                module.Types.Add((boolean = new(new("Boolean"))).Definition);

                module.Types.Add((int32 = new(new("Int32"))).Definition);

                module.Types.Add((float32 = new(new("Float32"))).Definition);

                module.Types.Add(array = new("Array"));
                module.Types.Add(reference = new("Reference"));
                module.Types.Add(pointer = new("Pointer"));
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

                Array = array,
                Reference = reference,
                Pointer = pointer,
            });
        }
    }
}
