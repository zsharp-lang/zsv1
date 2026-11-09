using ZSharp.Importer.ILLoader;

namespace Core.Runtime
{
    /// <summary>
    /// The operators of the platform's own types, as C# predefines them. Each method is an
    /// overload of the function its operator stands for. Smaller integers have none of
    /// their own: they widen to <c>Int32</c>, as C# promotes them.
    /// </summary>
    partial class ModuleScope
    {
        [Operator("+")] public static int Add(int left, int right) => left + right;
        [Operator("|")] public static int Or(int left, int right) => left | right;
        [Operator("&")] public static int And(int left, int right) => left & right;
        [Operator("|")] public static long Or(long left, long right) => left | right;
        [Operator("&")] public static long And(long left, long right) => left & right;

        /// <summary>
        /// Flags of an enum combine as its underlying integers do, as in C#.
        /// </summary>
        [Operator("|")] public static T Or<T>(T left, T right) where T : struct, Enum
            => (T)Enum.ToObject(typeof(T), Bits(left) | Bits(right));

        [Operator("&")] public static T And<T>(T left, T right) where T : struct, Enum
            => (T)Enum.ToObject(typeof(T), Bits(left) & Bits(right));

        private static ulong Bits(Enum value)
            => Convert.GetTypeCode(value) is TypeCode.SByte or TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64
                ? unchecked((ulong)Convert.ToInt64(value))
                : Convert.ToUInt64(value);
        [Operator("-")] public static int Subtract(int left, int right) => left - right;
        [Operator("*")] public static int Multiply(int left, int right) => left * right;
        [Operator("/")] public static int Divide(int left, int right) => left / right;
        [Operator("<")] public static bool LessThan(int left, int right) => left < right;
        [Operator(">")] public static bool GreaterThan(int left, int right) => left > right;
        [Operator("<=")] public static bool AtMost(int left, int right) => left <= right;
        [Operator(">=")] public static bool AtLeast(int left, int right) => left >= right;
        [Operator("==")] public static bool Equal(int left, int right) => left == right;
        [Operator("!=")] public static bool NotEqual(int left, int right) => left != right;

        [Operator("+")] public static uint Add(uint left, uint right) => left + right;
        [Operator("-")] public static uint Subtract(uint left, uint right) => left - right;
        [Operator("*")] public static uint Multiply(uint left, uint right) => left * right;
        [Operator("/")] public static uint Divide(uint left, uint right) => left / right;
        [Operator("<")] public static bool LessThan(uint left, uint right) => left < right;
        [Operator(">")] public static bool GreaterThan(uint left, uint right) => left > right;
        [Operator("<=")] public static bool AtMost(uint left, uint right) => left <= right;
        [Operator(">=")] public static bool AtLeast(uint left, uint right) => left >= right;
        [Operator("==")] public static bool Equal(uint left, uint right) => left == right;
        [Operator("!=")] public static bool NotEqual(uint left, uint right) => left != right;

        [Operator("+")] public static long Add(long left, long right) => left + right;
        [Operator("-")] public static long Subtract(long left, long right) => left - right;
        [Operator("*")] public static long Multiply(long left, long right) => left * right;
        [Operator("/")] public static long Divide(long left, long right) => left / right;
        [Operator("<")] public static bool LessThan(long left, long right) => left < right;
        [Operator(">")] public static bool GreaterThan(long left, long right) => left > right;
        [Operator("<=")] public static bool AtMost(long left, long right) => left <= right;
        [Operator(">=")] public static bool AtLeast(long left, long right) => left >= right;
        [Operator("==")] public static bool Equal(long left, long right) => left == right;
        [Operator("!=")] public static bool NotEqual(long left, long right) => left != right;

        [Operator("+")] public static ulong Add(ulong left, ulong right) => left + right;
        [Operator("-")] public static ulong Subtract(ulong left, ulong right) => left - right;
        [Operator("*")] public static ulong Multiply(ulong left, ulong right) => left * right;
        [Operator("/")] public static ulong Divide(ulong left, ulong right) => left / right;
        [Operator("<")] public static bool LessThan(ulong left, ulong right) => left < right;
        [Operator(">")] public static bool GreaterThan(ulong left, ulong right) => left > right;
        [Operator("<=")] public static bool AtMost(ulong left, ulong right) => left <= right;
        [Operator(">=")] public static bool AtLeast(ulong left, ulong right) => left >= right;
        [Operator("==")] public static bool Equal(ulong left, ulong right) => left == right;
        [Operator("!=")] public static bool NotEqual(ulong left, ulong right) => left != right;

        [Operator("+")] public static float Add(float left, float right) => left + right;
        [Operator("-")] public static float Subtract(float left, float right) => left - right;
        [Operator("*")] public static float Multiply(float left, float right) => left * right;
        [Operator("/")] public static float Divide(float left, float right) => left / right;
        [Operator("<")] public static bool LessThan(float left, float right) => left < right;
        [Operator(">")] public static bool GreaterThan(float left, float right) => left > right;
        [Operator("<=")] public static bool AtMost(float left, float right) => left <= right;
        [Operator(">=")] public static bool AtLeast(float left, float right) => left >= right;
        [Operator("==")] public static bool Equal(float left, float right) => left == right;
        [Operator("!=")] public static bool NotEqual(float left, float right) => left != right;

        [Operator("+")] public static double Add(double left, double right) => left + right;
        [Operator("-")] public static double Subtract(double left, double right) => left - right;
        [Operator("*")] public static double Multiply(double left, double right) => left * right;
        [Operator("/")] public static double Divide(double left, double right) => left / right;
        [Operator("<")] public static bool LessThan(double left, double right) => left < right;
        [Operator(">")] public static bool GreaterThan(double left, double right) => left > right;
        [Operator("<=")] public static bool AtMost(double left, double right) => left <= right;
        [Operator(">=")] public static bool AtLeast(double left, double right) => left >= right;
        [Operator("==")] public static bool Equal(double left, double right) => left == right;
        [Operator("!=")] public static bool NotEqual(double left, double right) => left != right;

        [Operator("+")] public static string Add(string left, string right) => left + right;

        // A string concatenates with any value, as C# has it.
        [Operator("+")] public static string Add(string left, object? right) => left + right;
        [Operator("+")] public static string Add(object? left, string right) => left + right;

        [Operator("or")] public static bool Or(bool left, bool right) => left || right;
        [Operator("==")] public static bool Equal(bool left, bool right) => left == right;
        [Operator("!=")] public static bool NotEqual(bool left, bool right) => left != right;
    }
}
