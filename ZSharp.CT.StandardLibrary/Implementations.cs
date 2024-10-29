using ZSharp.Compiler;
using ZSharp.VM;

namespace ZSharp.CT.StandardLibrary
{
    internal static class Implementations
    {
        public static ZSObject? Print(ZSObject[] arguments)
        {
            if (arguments.Length != 1)
                throw new();

            var value = arguments[0];

            Console.WriteLine(value is ZSString @string ? @string.Value : value);

            return null;
        }
    }
}
