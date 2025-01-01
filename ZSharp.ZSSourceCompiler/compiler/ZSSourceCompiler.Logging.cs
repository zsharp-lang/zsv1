namespace ZSharp.ZSSourceCompiler
{
    public sealed partial class ZSSourceCompiler : Compiler.Feature
    {
        public void LogError(string message, Node origin)
        {
            Compiler.Log.Error(message, new NodeLogOrigin(origin));
        }
    }
}
