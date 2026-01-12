namespace ZSharp.SourceCompiler.Module
{
    partial class ModuleCompiler
    {
        private readonly List<Logging.Log<string>> logs = [];

        public void Error(string message, Logging.LogOrigin origin)
            => logs.Add(new()
            {
                Level = Logging.LogLevel.Error,
                Message = message,
                Origin = origin
            });

        public void Error(string message, AST.Node node)
            => Error(message, new NodeLogOrigin(node));
    }
}
