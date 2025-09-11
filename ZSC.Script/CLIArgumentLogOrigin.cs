using ZSharp.Logging;

namespace ZSC.Script
{
    internal class CLIArgumentLogOrigin(string argument) : LogOrigin
    {
        public override string ToString()
            => $"CLI Argument '{argument}'";
    }
}
