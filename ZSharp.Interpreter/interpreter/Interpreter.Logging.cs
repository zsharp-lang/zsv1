using ZSharp.Logging;

namespace ZSharp.Interpreter
{
    partial class Interpreter
    {
        public Logger<string> Log => Compiler.Log;
    }
}
