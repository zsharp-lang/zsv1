namespace Core.Compiler.CO.TS
{
    partial class ModuleScope
    {
        [Alias("print")]
        public static void Print(string message)
            => System.Console.WriteLine(message);
    }
}
