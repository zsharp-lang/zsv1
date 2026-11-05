using ZSharp.Importer.ILLoader;

namespace Standard.IO
{
    public static class ModuleScope
    {
        [Alias("print")]
        public static void Print(object value)
            => Console.WriteLine(value);
    }
}
