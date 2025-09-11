namespace ZSharp.Objects
{
    internal static partial class Errors
    {
        public static string UndefinedGlobalType(string name)
            => $"Type of global {name} is not defined";

        public static string UndefinedReturnType(string name)
            => $"Return type of function {(name == string.Empty ? "<AnonymousFunction>" : name)} is not defined.";
    }
}
