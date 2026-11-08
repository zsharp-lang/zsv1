namespace ZSharp.Compiler.CGDispatchers.Direct
{
    partial class Dispatcher
    {
        public IResult Declare(CompilerObject definition)
        {
            var result = @base.Declare(definition);

            if (result.IsUnsupported() && definition.Is<ICTDeclare>(out var declare))
                result = declare.Declare();

            return result;
        }
    }
}
