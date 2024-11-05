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

        public static ZSObject AddFloat32(ZSObject[] arguments)
        {
            if (arguments.Length != 2) throw new();

            if (arguments[0] is not ZSFloat32 left || arguments[1] is not ZSFloat32 right) throw new();

            return new ZSFloat32(left.Value + right.Value, left.Type);
        }

        public static ZSObject AddInt32(ZSObject[] arguments)
        {
            if (arguments.Length != 2) throw new();

            if (arguments[0] is not ZSInt32 left || arguments[1] is not ZSInt32 right) throw new();

            return new ZSFloat32(left.Value + right.Value, left.Type);
        }
    }
}
