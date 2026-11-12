namespace ZSharp.Compiler.CGDispatchers.Typed
{
    partial class Dispatcher
    {
        public IResult<MatchResult, Error> Match(CompilerObject @object, CompilerObject pattern)
        {
            var result = @base.Match(@object, pattern);

            if (result.IsUnsupported() && RuntimeDescriptors.Find(ts, @object, out var rtd) && rtd.Is<IRTTypeMatch>(out var match))
                result = match.Match(@object, pattern);

            return result;
        }
    }
}
