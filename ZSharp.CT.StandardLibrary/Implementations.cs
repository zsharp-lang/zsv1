using ZSharp.VM;

namespace ZSharp.CT.StandardLibrary
{
    internal static class Implementations
    {
        public static ZSObject? standardModule;

        public static ZSObject? Print(ZSObject[] arguments)
        {
            if (arguments.Length != 1)
                throw new();

            var value = arguments[0];

            Console.WriteLine(value is ZSString @string ? @string.Value : value);

            return null;
        }

        public static ZSObject? Import(ZSObject[] arguments)
        {
            if (standardModule is null) throw new();

            if (arguments.Length != 1)
                throw new();

            var value = arguments[0];

            if (value is not ZSString zsString)
                throw new();

            if (zsString.Value != "std")
                throw new NotImplementedException();

            return standardModule;
        }
    }
}
