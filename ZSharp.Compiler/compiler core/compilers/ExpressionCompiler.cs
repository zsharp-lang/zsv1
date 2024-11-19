﻿using ZSharp.RAST;

namespace ZSharp.Compiler
{
    public sealed class ExpressionCompiler(Compiler compiler)
        : CompilerBase(compiler)
    {
        public CGObject Compile(RExpression expression)
            => expression switch
            {
                RBinaryExpression binary => Compile(binary),
                RCall call => Compile(call),
                RDefinition definition => Compiler.CompileNode(definition),
                RId id => Context.CurrentScope.Cache(id.Name) ?? throw new("Name not found " + id.Name),
                RLiteral literal => Compiler.CompileNode(literal),
                _ => throw new NotImplementedException(),
            };

        private CGObject Compile(RBinaryExpression binary)
        {
            var left = Compiler.CompileNode(binary.Left);

            // TODO: operator . should be implemented as a CGF
            if (binary.Operator == ".")
            {
                if (binary.Right is not RLiteral literal)
                    throw new();

                if (literal.Type == RLiteralType.String)
                    return Compiler.Member(left, literal.As<string>());
                else if (literal.Type == RLiteralType.Integer)
                    return Compiler.Member(left, literal.As<int>());
                else throw new();
            }
            else if (binary.Operator == "=") // TODO: implement as CGF
            {
                return Compiler.Assign(left, Compiler.CompileNode(binary.Right));
            }

            var right = Compiler.CompileNode(binary.Right);
            if (!Compiler.Operators.Cache(binary.Operator, out var @operator))
                throw new($"Could not find operator '{binary.Operator}'");

            return Compiler.Call(@operator, [
                new(left),
                new(right)
            ]);
        }

        private CGObject Compile(RCall call)
        {
            List<Argument> arguments = [];

            foreach (var argument in call.Arguments)
                arguments.Add(new(argument.Name, Compiler.CompileNode(argument.Value)));

            var callee = Compiler.CompileNode(call.Callee);

            return Compiler.Call(callee, [.. arguments]);
        }
    }
}
