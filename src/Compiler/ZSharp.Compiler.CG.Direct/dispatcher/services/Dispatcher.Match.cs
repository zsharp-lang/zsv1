namespace ZSharp.Compiler.CGDispatchers.Direct
{
    partial class Dispatcher
    {
        public IResult<MatchResult, Error> Match(CompilerObject @object, CompilerObject pattern)
        {
            var result = @base.Match(@object, pattern);

            if (result.IsUnsupported() && @object.Is<ICTTypeMatch>(out var match))
                result = match.Match(pattern);

            return result;
        }
    }
}
