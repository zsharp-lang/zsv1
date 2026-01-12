using ZSharp.Logging;

namespace ZSharp.CLI
{
    internal class CLIArgumentLogOrigin(string argument) : LogOrigin
    {
        public override string ToString()
            => $"CLI Argument '{argument}'";
    }
}
