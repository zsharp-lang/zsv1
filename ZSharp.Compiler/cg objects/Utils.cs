using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public static class Utils
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
