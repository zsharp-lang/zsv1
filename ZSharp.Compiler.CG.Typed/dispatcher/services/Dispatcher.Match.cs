namespace ZSharp.Compiler.Dispatchers.Typed
{
    partial class Dispatcher
    {
        public Result<MatchResult> Match(CompilerObject @object, CompilerObject pattern)
        {
            var result = @base.Match(@object, pattern);

            if (result.IsError && compiler.RuntimeDescriptor(@object, out var rtd) && rtd.Is<IRTTypeMatch>(out var match))
                result = match.Match(compiler, @object, pattern);

            return result;
        }
    }
}
