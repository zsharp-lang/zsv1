using System.Reflection;
using System.Reflection.Emit;

namespace ZSharp.Importer.ILLoader
{
    /// <summary>
    /// The conversions C# has between CLR types: numeric ones, implicit where no value is
    /// lost and explicit otherwise, and those a type defines as <c>op_Implicit</c> and
    /// <c>op_Explicit</c>.
    /// </summary>
    internal static class Conversions
    {
        private static readonly Dictionary<Type, Type[]> widening = new()
        {
            [typeof(sbyte)] = [typeof(short), typeof(int), typeof(long), typeof(float), typeof(double)],
            [typeof(byte)] = [typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double)],
            [typeof(short)] = [typeof(int), typeof(long), typeof(float), typeof(double)],
            [typeof(ushort)] = [typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double)],
            [typeof(int)] = [typeof(long), typeof(float), typeof(double)],
            [typeof(uint)] = [typeof(long), typeof(ulong), typeof(float), typeof(double)],
            [typeof(long)] = [typeof(float), typeof(double)],
            [typeof(ulong)] = [typeof(float), typeof(double)],
            [typeof(char)] = [typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double)],
            [typeof(float)] = [typeof(double)],
        };

        private static readonly HashSet<Type> unsigned = [typeof(byte), typeof(ushort), typeof(uint), typeof(ulong), typeof(char)];

        public static bool IsNumeric(Type type) => widening.ContainsKey(type) || type == typeof(double);

        public static bool Widens(Type from, Type to)
            => widening.TryGetValue(from, out var targets) && targets.Contains(to);

        /// <summary>
        /// The code that converts a number already on the stack to another numeric type.
        /// </summary>
        public static void Emit(ILGenerator il, Type from, Type to)
        {
            if (to == typeof(float) || to == typeof(double))
            {
                if (unsigned.Contains(from) && from != typeof(char))
                    il.Emit(OpCodes.Conv_R_Un);

                il.Emit(to == typeof(float) ? OpCodes.Conv_R4 : OpCodes.Conv_R8);
                return;
            }

            var opcode = to switch
            {
                _ when to == typeof(sbyte) => OpCodes.Conv_I1,
                _ when to == typeof(byte) => OpCodes.Conv_U1,
                _ when to == typeof(short) => OpCodes.Conv_I2,
                _ when to == typeof(ushort) || to == typeof(char) => OpCodes.Conv_U2,
                _ when to == typeof(int) => OpCodes.Conv_I4,
                _ when to == typeof(uint) => OpCodes.Conv_U4,
                _ when to == typeof(long) => unsigned.Contains(from) ? OpCodes.Conv_U8 : OpCodes.Conv_I8,
                _ when to == typeof(ulong) => OpCodes.Conv_U8,
                _ => throw new NotSupportedException($"{to.Name} is not a number.")
            };

            il.Emit(opcode);
        }

        /// <summary>
        /// A conversion a type defines from one type to another, with the name C# gives it.
        /// </summary>
        public static MethodInfo? Defined(Type from, Type to, string name)
            => new[] { from, to }
                .Distinct()
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                .FirstOrDefault(method => method.Name == name
                    && method.ReturnType == to
                    && method.GetParameters() is [var parameter]
                    && parameter.ParameterType.IsAssignableFrom(from));
    }
}
