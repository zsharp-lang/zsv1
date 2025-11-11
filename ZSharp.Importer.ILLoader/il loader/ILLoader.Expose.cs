using CommonZ.Utils;
using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Importer.ILLoader
{
    public sealed partial class ILLoader
    {
        [MaybeNull]
        private IR.Function castFunction;

        public Collection<IR.VM.Instruction> ExposeAsIR<T>(T? obj)
            => ExposeAsIR(obj, typeof(T));

        public Collection<IR.VM.Instruction> ExposeAsIR(object? obj)
            => ExposeAsIR(obj, obj?.GetType() ?? typeof(object));

        public Collection<IR.VM.Instruction> ExposeAsIR(object? obj, Type type)
        {
            var rt = RequireRuntime();

            if (obj is null) return rt.Expose(null);

            var coType = LoadType(type);
            var irType = IR.CompileType(coType, rt).Unwrap();

            var code = rt.Expose(obj);

            var function = new IR.ConstructedFunction(castFunction!);
            function.Arguments.Add(irType);

            code.Add(new IR.VM.Call(function));
            return code;
        }

        public CompilerObject Expose<T>(T? obj)
            => Expose(obj, typeof(T));

        public CompilerObject Expose(object? obj)
            => Expose(obj, obj?.GetType() ?? typeof(object));

        public CompilerObject Expose(object? obj, Type type)
            => new RawIRCode(new([.. ExposeAsIR(obj, type)])
            {
                Types = [IR.CompileType(LoadType(type)).Unwrap()]
            });

        public CompilerObject Expose(Delegate @delegate)
        {
            //var wrapped = new ExposedObjectWrapper(@delegate.Target, @delegate.Method);
            // TODO: Cache this method info
            // TODO: Add support for array. Currently not supported because there's no make-array instruction in IR
            //var methodCO = LoadMethod(typeof(ExposedObjectWrapper).GetMethod("Invoke", [typeof(object)]) ?? throw new());
            //var @object = ExposeAsIR(wrapped);
            if (@delegate.Target is null)
                return LoadMethod(@delegate.Method);

            var @object = ExposeAsIR(@delegate.Target);
            var methodCO = LoadMethod(@delegate.Method);
            var objectCode = RawIRCode.From(@object, LoadType(@delegate.Target.GetType()));

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

            var castFunctionReturnType = new IR.GenericParameter("T");
            castFunction = new(castFunctionReturnType);
            castFunction.GenericParameters.Add(castFunctionReturnType);
            castFunction.Signature.Args.Parameters.Add(new("obj", rt.TypeSystem.Object));

            rt.AddFunction(castFunction, ((Delegate)Cast<object>).Method.GetGenericMethodDefinition());
        }

        public static T Cast<T>(object obj)
            => (T)obj;
    }
}
