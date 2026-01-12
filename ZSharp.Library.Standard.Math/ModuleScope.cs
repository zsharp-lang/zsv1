namespace Standard.Math
{
    [ModuleScope]
    public static class ModuleScope
    {
        [Alias("ceil")]
        public static int Ceil(float x)
            => (int)System.Math.Ceiling(x);

        [Alias("log2")]
        public static float Log2(int x)
            => (float)System.Math.Log(x, 2);

        [Alias("random")]
        public static int Random(int min, int max)
            => new System.Random().Next(min, max);

        [Operator("+", Kind = OperatorKind.Infix)]
        public static int Add(int a, int b)
            => a + b;

        [Operator("<", Kind = OperatorKind.Infix)]
        public static bool LessThan(int a, int b)
            => a < b;

        [Operator("-", Kind = OperatorKind.Infix)]
        public static int Subtract(int a, int b)
            => a - b;
    }
}
