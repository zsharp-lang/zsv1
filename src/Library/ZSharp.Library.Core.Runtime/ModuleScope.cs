using ZSharp.Importer.ILLoader;

namespace Core.Runtime
{
    /// <summary>
    /// The running platform as seen through <c>core:runtime</c>. The CLI sets these
    /// once it has created them.
    /// </summary>
    [ModuleScope]
    public static partial class ModuleScope
    {
        [Alias("runtime")]
        public static ZSharp.Platform.Runtime.Runtime Runtime { get; set; } = null!;

        [Alias("ilLoader")]
        public static ILLoader ILLoader { get; set; } = null!;

        /// <summary>
        /// What imports an assembly of the running platform as its module, as importing
        /// its file does. The host sets it, with what imports a <c>.dll</c>.
        /// </summary>
        public static Assemblies Assemblies { get; set; } = null!;

        /// <summary>
        /// Imports an assembly the running platform has, as its module, so what reaches the
        /// assembly needn't find its file: <c>import { BindingFlags } from Object.Assembly</c>.
        /// </summary>
        [Operator("import")]
        public static ZSharp.HIR.Instance Import(System.Reflection.Assembly assembly)
            => Assemblies.Import(assembly);

        /// <summary>
        /// The operator the CLR has for a piece of syntax and the types of its operands, as
        /// its method: one of these, as <c>Add</c> is of <c>+</c> for <c>Int32</c>, or else
        /// the one the first operand's type defines, as <c>Half</c>'s <c>op_Addition</c>.
        /// What a platform implements an operator declared for Z#'s types with, by the CLR
        /// types they are. <see langword="null"/> if the CLR has none.
        /// </summary>
        [Alias("predefined")]
        public static ZSharp.HIR.Instance? Predefined(string syntax, Type[] operands)
        {
            const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static;

            var method = typeof(ModuleScope).GetMethods(flags).FirstOrDefault(method =>
                method.GetCustomAttributes(typeof(OperatorAttribute), false).Cast<OperatorAttribute>().Any(attribute => attribute.Operator == syntax)
                && method.GetParameters().Select(parameter => parameter.ParameterType).SequenceEqual(operands));

            if (method is null && operands.Length > 0 && Defined(syntax, operands.Length) is { } name)
                method = operands[0].GetMethod(name, flags, operands);

            return method is null ? null : ILLoader.Load(method);
        }

        /// <summary>
        /// The name C# gives the method a type defines for an operator.
        /// </summary>
        private static string? Defined(string syntax, int operands)
            => (syntax, operands) switch
            {
                ("+", 2) => "op_Addition",
                ("-", 2) => "op_Subtraction",
                ("-", 1) => "op_UnaryNegation",
                ("*", 2) => "op_Multiply",
                ("/", 2) => "op_Division",
                ("<", 2) => "op_LessThan",
                (">", 2) => "op_GreaterThan",
                ("<=", 2) => "op_LessThanOrEqual",
                (">=", 2) => "op_GreaterThanOrEqual",
                ("==", 2) => "op_Equality",
                ("!=", 2) => "op_Inequality",
                _ => null
            };
    }
}
