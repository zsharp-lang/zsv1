using ZSharp.Importer.ILLoader;

namespace Core.Runtime
{
    /// <summary>
    /// The operators of the platform's own types. Each method is an overload of the
    /// function its operator stands for.
    /// </summary>
    partial class ModuleScope
    {
        [Operator("+")] public static int Add(int left, int right) => left + right;
        [Operator("-")] public static int Subtract(int left, int right) => left - right;
        [Operator("*")] public static int Multiply(int left, int right) => left * right;
        [Operator("/")] public static int Divide(int left, int right) => left / right;
        [Operator("<")] public static bool LessThan(int left, int right) => left < right;
        [Operator("==")] public static bool Equal(int left, int right) => left == right;
        [Operator("!=")] public static bool NotEqual(int left, int right) => left != right;

        [Operator("+")] public static double Add(double left, double right) => left + right;
        [Operator("-")] public static double Subtract(double left, double right) => left - right;
        [Operator("*")] public static double Multiply(double left, double right) => left * right;
        [Operator("/")] public static double Divide(double left, double right) => left / right;
        [Operator("<")] public static bool LessThan(double left, double right) => left < right;
        [Operator("==")] public static bool Equal(double left, double right) => left == right;
        [Operator("!=")] public static bool NotEqual(double left, double right) => left != right;

        [Operator("+")] public static string Add(string left, string right) => left + right;
        [Operator("==")] public static bool Equal(string left, string right) => left == right;
        [Operator("!=")] public static bool NotEqual(string left, string right) => left != right;

        [Operator("or")] public static bool Or(bool left, bool right) => left || right;
        [Operator("==")] public static bool Equal(bool left, bool right) => left == right;
        [Operator("!=")] public static bool NotEqual(bool left, bool right) => left != right;
    }
}
