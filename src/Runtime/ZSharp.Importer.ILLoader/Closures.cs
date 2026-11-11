using System.Linq.Expressions;

namespace ZSharp.Importer.ILLoader
{
    /// <summary>
    /// Makes a local function a value: a delegate bound to the cells of one run of the
    /// code around it, as a C# lambda is bound to its closure. Emitted code calls these.
    /// </summary>
    public static class Closures
    {
        public static Binding Bind(object function)
            => new((LocalFunctionValue)function, []);

        public static Binding With(Binding binding, object cell)
            => new(binding.Function, [.. binding.Cells, cell]);

        public static Delegate Make(Binding binding)
        {
            var method = binding.Function.Runtime.LoadMethod(binding.Function.Lowered);
            var parameters = method.GetParameters();

            // The delegate takes what its type says, which goes to what the function takes.
            var arguments = binding.Function.Type.GetMethod("Invoke")!
                .GetParameters()
                .Select(parameter => Expression.Parameter(parameter.ParameterType, parameter.Name))
                .ToArray();
            var passed = arguments.Select((argument, index) =>
            {
                var type = parameters[binding.Cells.Length + index].ParameterType;

                return argument.Type == type ? (Expression)argument : Expression.Convert(argument, type);
            });
            var cells = binding.Cells.Select((cell, index) => Expression.Constant(cell, parameters[index].ParameterType));

            return Expression.Lambda(
                binding.Function.Type,
                Expression.Call(method, [.. cells, .. passed]),
                arguments
            ).Compile();
        }

        public sealed class Binding(LocalFunctionValue function, object[] cells)
        {
            internal LocalFunctionValue Function { get; } = function;

            internal object[] Cells { get; } = cells;
        }
    }

    /// <summary>
    /// A local function as emitted code refers to it, with the delegate type its values have.
    /// </summary>
    public sealed class LocalFunctionValue(Platform.Runtime.Runtime runtime, LIR.Function lowered, Type type)
    {
        internal Platform.Runtime.Runtime Runtime { get; } = runtime;

        internal LIR.Function Lowered { get; } = lowered;

        internal Type Type { get; } = type;
    }
}
