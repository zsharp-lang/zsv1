using ZSharp.Logging;

namespace ZSC.Script
{
    internal sealed class ZSCScriptLogOrigin() : LogOrigin
    {
        public static ZSCScriptLogOrigin Instance = new();

        public override string ToString()
            => "[Z# Script Compiler]";
    }
}
