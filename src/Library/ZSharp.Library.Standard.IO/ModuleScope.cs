using ZSharp.Importer.ILLoader;

namespace Standard.IO
{
    [ModuleScope]
    public static class ModuleScope
    {
        [Alias("print")]
        public static void Print(object value)
            => Console.WriteLine(value);
    }
}
