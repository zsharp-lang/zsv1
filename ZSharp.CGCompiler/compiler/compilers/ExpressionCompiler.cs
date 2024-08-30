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
                RBinaryExpression binary => Compile(binary),
                RCall call => Compile(call),
                RDefinition definition => Compile(definition),
                RId id => Compile(id),
                RLiteral literal => Compile(literal),
                _ => throw new NotImplementedException(),
            };

        private CGCode Compile(RBinaryExpression binary)
        {
            // TODO: implement member access operator as regular operator
            // but the normal overload should actually be a CT function
            if (binary.Operator == ".")
            {
                if (binary.Right is not RLiteral literal || literal.Type != RLiteralType.String)
                    throw new Exception("Member access operator requires a string literal as the right operand.");

                return [
                    ..Compile(binary.Left),
                    CG.GetMember((literal.Value as string)!)
                ];
            }
            else return Compile(
                new RCall(
                    new RId(binary.Operator),
                    [
                        new(binary.Left),
                        new(binary.Right)
                    ]
                )
            );
        }

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
