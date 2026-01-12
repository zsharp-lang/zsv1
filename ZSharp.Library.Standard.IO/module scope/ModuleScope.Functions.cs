namespace Standard.IO
{
    partial class ModuleScope
    {
        [Alias("print")]
        public static void Print(object value)
            => System.Console.WriteLine(value);
    }
}
