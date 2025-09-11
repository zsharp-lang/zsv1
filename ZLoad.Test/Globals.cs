using ZSharp.Runtime.NET.IL2IR;

namespace ZLoad.Test
{
    [ModuleGlobals]
    public static partial class Globals
    {
        [Alias(Name = "greet")]
        public static string Greet(string name)
            => $"Hello, {name}!";

        [Alias(Name = "id")]
        public static T Id<T>(T v)
            => v;
    }
}
