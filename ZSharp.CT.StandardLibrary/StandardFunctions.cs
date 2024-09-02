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
                            Type = [CGRuntime.CG.Get("string")],
                            IR = parameter
                        }
                    ]
                }
            };
        }
    }
}
