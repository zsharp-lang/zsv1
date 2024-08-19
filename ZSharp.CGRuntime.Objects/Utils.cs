using CommonZ.Utils;
using ZSharp.CGRuntime;

namespace ZSharp.Compiler
{
    internal static class Utils
    {
        public static (Args, KwArgs) SplitArguments(Argument[] arguments)
        {
            Args positional = [];
            KwArgs named = [];

            foreach (var argument in arguments)
            {
                if (argument.Name is null)
                    positional.Add(argument.Object);
                else
                    named[argument.Name] = argument.Object;
            }

            return (positional, named);
        }
    }
}
