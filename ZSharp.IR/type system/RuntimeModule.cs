﻿namespace ZSharp.IR
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

            {
                module.Types.Add(type = new("Type"));
                module.Types.Add(@string = new("String"));
                module.Types.Add(@void = new("Void"));
            }

            return new(module, new()
            {
                String = @string,
                Type = type,
                Void = @void,
            });
        }
    }
}
