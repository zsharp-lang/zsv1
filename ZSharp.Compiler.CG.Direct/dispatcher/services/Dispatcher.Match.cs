namespace ZSharp.Compiler.CGDispatchers.Direct
{
    partial class Dispatcher
    {
        public IResult<MatchResult, Error> Match(CompilerObject @object, CompilerObject pattern)
        {
            var result = @base.Match(@object, pattern);

            if (result.IsError && @object.Is<ICTTypeMatch>(out var match))
                result = match.Match(compiler, pattern);

            return result;
        }
    }
}
