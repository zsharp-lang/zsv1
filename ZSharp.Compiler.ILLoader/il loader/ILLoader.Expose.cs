using CommonZ.Utils;
using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Compiler.ILLoader
{
    public sealed partial class ILLoader
    {
        [MaybeNull]
        private ZSharp.IR.Function castFunction;

        public Collection<ZSharp.IR.VM.Instruction> ExposeAsIR<T>(T? obj)
            => ExposeAsIR(obj, typeof(T));

        public Collection<ZSharp.IR.VM.Instruction> ExposeAsIR(object? obj)
            => ExposeAsIR(obj, obj?.GetType() ?? typeof(object));

        public Collection<ZSharp.IR.VM.Instruction> ExposeAsIR(object? obj, Type type)
        {
            var rt = RequireRuntime();
            var ir = RequireIR();

            if (obj is null) return rt.Expose(null);

            var coType = LoadType(type);
            var irType = ir.CompileType(coType).Unwrap();

            var code = rt.Expose(obj);

            var function = new ZSharp.IR.GenericFunctionInstance(castFunction!);
            function.Arguments.Add(irType);

            code.Add(new ZSharp.IR.VM.Call(function));
            return code;
        }

        public CompilerObject Expose<T>(T? obj)
            => Expose(obj, typeof(T));

        public CompilerObject Expose(object? obj)
            => Expose(obj, obj?.GetType() ?? typeof(object));

        public CompilerObject Expose(object? obj, Type type)
            => new RawIRCode(new([.. ExposeAsIR(obj, type)])
            {
                Types = [RequireIR().CompileType(LoadType(type)).Unwrap()]
            });

        public CompilerObject Expose(Delegate @delegate)
        {
            var methodCO = LoadMethod(@delegate.Method);
            var @object = ExposeAsIR(@delegate.Target);
            var objectCode = new RawIRCode(new(@object));

            return new Objects.BoundMethod()
            {
                Method = methodCO,
                Object = objectCode
            };
        }

        [MemberNotNull(nameof(castFunction))]
        private void SetupExposeSystem()
        {
            var rt = RequireRuntime();

            var castFunctionReturnType = new ZSharp.IR.GenericParameter("T");
            castFunction = new(castFunctionReturnType);
            castFunction.GenericParameters.Add(castFunctionReturnType);
            castFunction.Signature.Args.Parameters.Add(new("obj", rt.TypeSystem.Object));

            rt.AddFunction(castFunction, ((Delegate)Cast<object>).Method.GetGenericMethodDefinition());
        }

        public static T Cast<T>(object obj)
            => (T)obj;
    }
}
