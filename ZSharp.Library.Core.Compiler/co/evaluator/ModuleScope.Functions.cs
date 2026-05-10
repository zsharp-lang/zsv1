namespace Core.Compiler.CO.Evaluator
{
    partial class ModuleScope
    {
        [Alias("print")]
        public static void Print(string message)
            => System.Console.WriteLine(message);
    }
}
