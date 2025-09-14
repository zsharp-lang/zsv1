using ZSharp.Logging;

namespace ZSharp.CLI
{
    internal sealed class ZSCScriptLogOrigin() : LogOrigin
    {
        public static ZSCScriptLogOrigin Instance = new();

        public override string ToString()
            => "[Z# Script Compiler]";
    }
}
