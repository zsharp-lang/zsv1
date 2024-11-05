using ZSharp.VM;

namespace ZSharp.CT.StandardLibrary
{
    internal static class StandardFunctions
    {
        public static InternalFunction Print(Runtime runtime)
        {
            var ir = new IR.Function(runtime.RuntimeModule.TypeSystem.Void)
            {
                Name = "print"
            };

            IR.Parameter parameter;
            ir.Signature.Args.Parameters.Add(parameter = new("value", runtime.RuntimeModule.TypeSystem.String));

            return new(ir)
            {
                Implementation = Implementations.Print,
                Signature = new()
                {
                    Args = [
                        new("value") {
                            Type = null,
                            IR = parameter
                        }
                    ]
                }
            };
        }

        public static InternalFunction AddFloat32(Runtime runtime)
        {
            var ir = new IR.Function(runtime.RuntimeModule.TypeSystem.Float32)
            {
                Name = "+"
            };

            IR.Parameter left, right;
            ir.Signature.Args.Parameters.AddRange([
                left = new("left", runtime.RuntimeModule.TypeSystem.Float32),
                right = new("right", runtime.RuntimeModule.TypeSystem.Float32)
            ]);

            return new(ir)
            {
                Implementation = Implementations.AddFloat32,
                Signature = new()
                {
                    Args = [
                        new("left") {
                            Type = null!,
                            IR = left
                        },
                        new("right") {
                            Type = null!,
                            IR = right
                        }
                    ]
                }
            };
        }

        public static InternalFunction AddInt32(Runtime runtime)
        {
            var ir = new IR.Function(runtime.RuntimeModule.TypeSystem.Int32)
            {
                Name = "+"
            };

            IR.Parameter left, right;
            ir.Signature.Args.Parameters.AddRange([
                left = new("left", runtime.RuntimeModule.TypeSystem.Int32),
                right = new("right", runtime.RuntimeModule.TypeSystem.Int32)
            ]);

            return new(ir)
            {
                Implementation = Implementations.AddInt32,
                Signature = new()
                {
                    Args = [
                        new("left") {
                            Type = null!,
                            IR = left
                        },
                        new("right") {
                            Type = null!,
                            IR = right
                        }
                    ]
                }
            };
        }
    }
}
