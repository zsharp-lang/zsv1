namespace ZSharp.Compiler.CGDispatchers.Direct
{
    partial class Dispatcher
    {
        public IResult MatchParameter(CompilerObject parameter, ArgumentStream arguments)
        {
            var result = @base.MatchParameter(parameter, arguments);

            if (result.IsUnsupported() && parameter.Is<ICTMatchParameter>(out var match))
                result = match.Match(arguments);

            return result;
        }
    }
}
