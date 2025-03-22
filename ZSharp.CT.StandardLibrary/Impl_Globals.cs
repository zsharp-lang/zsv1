using ZSharp.Runtime.NET.IL2IR;


namespace Standard.IO
{
    [ModuleGlobals]
    public static class Impl_Globals
    {
        [Alias(Name = "print")]
        public static void Print(string value)
            => Console.WriteLine(value);

        [Alias(Name = "input")]
        public static string Input(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine()!;
        }

        [Alias(Name = "_+_")]
        public static string Concat(string a, string b)
            => a + b;

        [Alias(Name = "string()")]
        public static string ToString(int x)
            => x.ToString();

        [Alias(Name = "int32.parse()")]
        public static int ParseInt32(string s)
            => int.Parse(s);

        [Alias(Name = "_==_")]
        public static bool Equals(string a, string b)
            => a == b;

        [Alias(Name = "_==_")]
        public static bool Equals(bool a, bool b)
            => a == b;

        [Alias(Name = "_!=_")]
        public static bool NotEquals(string a, string b)
            => a != b;
    }
}
