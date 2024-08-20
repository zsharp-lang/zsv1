using ZSharp.CGRuntime;
using ZSharp.RAST;

namespace ZSharp.CGCompiler
{
    internal sealed class ExpressionCompiler(Context context)
        : CompilerBase(context)
    {
        public CGCode Compile(RExpression expression)
            => expression switch
            {
                RCall call => Compile(call),
                RDefinition definition => Compile(definition),
                RId id => Compile(id),
                RLiteral literal => Compile(literal),
                _ => throw new NotImplementedException(),
            };

        private CGCode Compile(RCall call)
        {
            CGCode result = [.. Compile(call.Callee)];

            foreach (var argument in call.Arguments)
            {
                result.AddRange(Compile(argument.Value));
                result.Add(CG.Argument(argument.Name));
            }

            result.Add(CG.Call(call.Arguments.Length));

            return result;
        }

        private CGCode Compile(RDefinition definition)
            => [CG.Object(Context.Compile(definition))];

        private CGCode Compile(RId id)
            => [CG.Get(id.Name)];

        private CGCode Compile(RLiteral literal)
            => literal.UnitType is null 
            ? [CG.Literal(literal.Value, literal.Type)]
            : [CG.Literal(literal.Value, literal.Type), ..Compile(literal.UnitType)]
            ;
    }
}
