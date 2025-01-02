using ZSharp.Runtime.NET.IL2IR;

namespace Standard.Math
{
    [ModuleGlobals]
    public static class Impl_Globals
    {
        [Alias(Name = "ceil")]
        public static int Ceil(float x)
            => (int)System.Math.Ceiling(x);

        [Alias(Name = "log2")]
        public static float Log2(int x)
            => (float)System.Math.Log(x, 2);

        [Alias(Name = "random")]
        public static int Random(int min, int max)
            => new System.Random().Next(min, max);

        [Alias(Name = "_+_")]
        public static int Add(int a, int b)
            => a + b;

        [Alias(Name = "_<_")]
        public static bool LessThan(int a, int b)
            => a < b;

        [Alias(Name = "_-_")]
        public static int Subtract(int a, int b)
            => a - b;
    }
}
