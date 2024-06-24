using CommonZ.Utils;
using ZSharp.RAST;

namespace ZSharp.CTRuntime
{
    public sealed class Desugarer
    {
        public DesugaringContext Context { get; set; } = new();

        public RNode Desugar(RNode node)
        {
            RNode result;
            while (node != (result = node switch
            {
                RExpression expression => Desugar(expression),
                RStatement statement => Desugar(statement),
                _ => node
            }))
            {
                break;
            }
            return result;
        }

        private RExpression Desugar(RExpression expression)
        {
            return expression switch
            {
                RBinaryExpression binaryExpression => Desugar(binaryExpression),
                RDefinition definition => Desugar(definition),
                _ => expression
            };
        }

        private RExpression Desugar(RBinaryExpression binary)
        {
            if (binary.Operator == "." && binary.Right is RId id)
                binary.Right = RLiteral.String(id.Name);

            return new RCall(
                Context.GetBinaryOperator(binary.Operator),
                [
                    new(binary.Left), new(binary.Right)
                ]
            );
        }

        private RBlock Desugar(RBlock block)
        {
            return new(block.Statements.Select(Desugar).ToList()) { IsScoped = block.IsScoped };
        }

        private RDefinition Desugar(RDefinition definition)
        {
            return definition switch
            {
                RFunction function => Desugar(function),
                RModule module => Desugar(module),
                RParameter parameter => Desugar(parameter),
                _ => definition
            };
        }

        private RFunction Desugar(RFunction function)
        {
            function.Signature = Desugar(function.Signature);

            if (function.Body is not null)
                function.Body = Desugar(function.Body);

            return function;
        }

        private RBlock Desugar(RImport import)
        {
            var args = new Collection<RArgument>()
            {
                new(import.Source)
            };
            args.AddRange(
                import.Arguments?.Select(
                    arg => new RArgument(arg.Name, Desugar(arg.Value))
                ) ?? []
            );

            RCall importCall = new(
                Context.ImportFunction,
                [.. args]
                );

            RBlock result = new([])
            {
                IsScoped = false
            };

            //RLetDefinition callResult = new(import.As ?? new("import result"), null, importCall);
            RExpression callResult = import.As as RExpression ?? importCall;

            if (import.As is not null)
                result.Statements.Add(
                    new RExpressionStatement(
                        new RLetDefinition(import.As, null, importCall)
                    )
                );

            RStatement desugaredNode;

            if (import.Target is not null)
                desugaredNode = new RLetStatement(import.Target, null, callResult);
            else
                desugaredNode = new RExpressionStatement(callResult);

            result.Statements.Add(Desugar(desugaredNode));

            return result;
        }

        private RBlock Desugar(RLetStatement let)
        {
            //var exprTemp = new RLetDefinition("let statement temp", let.Type, let.Value);

            return new RBlock(
            [
                //new RExpressionStatement(exprTemp),
                Desugar(let.Pattern, let.Value, let.Type, true)
            ])
            {
                IsScoped = false
            };
        }

        public RModule Desugar(RModule module)
        {
            if (module.Content is not null)
                module.Content = Desugar(module.Content);
            return module;
        }

        private RParameter Desugar(RParameter parameter)
        {
            parameter.Type = parameter.Type is null ? null : Desugar(parameter.Type);
            parameter.Default = parameter.Default is null ? null : Desugar(parameter.Default);
            return parameter;
        }

        private RSignature Desugar(RSignature signature)
        {
            signature.Args = signature.Args?.Select(Desugar)?.ToList();

            signature.KwArgs = signature.KwArgs?.Select(Desugar)?.ToList();

            if (signature.VarArgs is not null)
                signature.VarArgs = Desugar(signature.VarArgs);

            if (signature.VarKwArgs is not null)
                signature.VarKwArgs = Desugar(signature.VarKwArgs);

            return signature;
        }

        private RStatement Desugar(RStatement statement)
        {
            return statement switch
            {
                RBlock block => Desugar(block),
                RImport import => Desugar(import),
                RLetStatement let => Desugar(let),
                _ => statement
            };
        }

        #region Patterns

        private RStatement Desugar(RPattern pattern, RExpression value, RExpression? type, bool readOnly)
        {
            return pattern switch
            {
                RNamePattern namePattern => Desugar(namePattern, value, type, readOnly),
                RObjectPattern objectPattern => Desugar(objectPattern, value, type, readOnly),
                _ => throw new Exception()
            };
        }

        private static RStatement Desugar(RNamePattern pattern, RExpression value, RExpression? type, bool readOnly)
        {
            RDefinition local = readOnly 
                ? new RLetDefinition(pattern.Id, pattern.Type ?? type, value) 
                : new RVarDefinition(pattern.Id, pattern.Type ?? type, value);
            return new RExpressionStatement(local);
        }

        private RStatement Desugar(RObjectPattern pattern, RExpression value, RExpression? type, bool readOnly)
        {
            if (type is not null)
                throw new NotImplementedException();

            List<RStatement> result = new();

            RLetDefinition temp = new("object pattern temp", null, value);
            result.Add(new RExpressionStatement(temp));

            foreach (var (name, member)in pattern.Patterns)
            {
                result.Add(Desugar(member, Desugar(new RBinaryExpression(temp.Id, new RId(name), ".")), null, readOnly));
            }

            return new RBlock(result) { IsScoped = false };
        }

        #endregion
    }
}
