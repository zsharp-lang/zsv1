using ZSharp.Compiler;

namespace ZSharp.ZSSourceCompiler
{
    public sealed partial class ExpressionCompiler(ZSSourceCompiler compiler)
        : CompilerBase(compiler)
    {
        public ObjectResult? CompileNode(Expression expression)
            => expression switch
            {
                ArrayLiteral array => Compile(array),
                BinaryExpression binary => Compile(binary),
                CallExpression call => Compile(call),
                CastExpression cast => Compile(cast),
                IdentifierExpression identifier => Compile(identifier),
                IndexExpression index => Compile(index),
                IsOfExpression isOf => Compile(isOf),
                LiteralExpression literal => Compile(literal),
                WhileExpression<Expression> @while => Compile(@while),
                _ => null
            };

        private ObjectResult Compile(BinaryExpression binary)
        {
            // TODO: NO SPECIAL TREATMENT FOR OPERATORS . and =

            var left = Compiler.CompileNode(binary.Left).Unwrap();

            if (binary.Operator == ".")
            {
                CompilerObject member;

                if (binary.Right is IdentifierExpression identifier)
                    member = Compiler.Compiler.CreateString(identifier.Name);

                else if (binary.Right is not LiteralExpression literal)
                    return Compiler.CompilationError("Expected a literal expression on the right side of the dot operator.", binary.Right);

                else member = Compile(literal).Unwrap();

                if (Compiler.Compiler.IsString(member, out var memberName))
                    return ObjectResult.Ok(Compiler.Compiler.CG.Member(left, memberName).Unwrap());
                if (Compiler.Compiler.IsLiteral<int>(member, out var memberIndex))
                    return ObjectResult.Ok(Compiler.Compiler.Member(left, memberIndex.Value));

                return Compiler.CompilationError("Expected a string or an integer literal on the right side of the dot operator.", binary.Right);
            }

            var right = Compiler.CompileNode(binary.Right).Unwrap();

            if (binary.Operator == "=")
                return ObjectResult.Ok(Compiler.Compiler.CG.Set(left, right).Unwrap());

            if (!Compiler.Operators.Binary.Cache(binary.Operator, out var @operator))
                return Compiler.CompilationError($"Operator '{binary.Operator}' is not defined.", binary);

            return ObjectResult.Ok(
                Compiler.Compiler.CG.Call(@operator, [new(left), new(right)]).Unwrap()
            );
        }

        private ObjectResult Compile(CallExpression call)
        {
            var callable = Compiler.Compiler.Evaluate(Compiler.CompileNode(call.Callee).Unwrap());

            var args = call.Arguments.Select(arg => new Argument_NEW<CompilerObject>(arg.Name, Compiler.CompileNode(arg.Value).Unwrap()));

            return ObjectResult.Ok(
                Compiler.Compiler.CG.Call(callable, args.ToArray()).Unwrap()
            );
        }

        private ObjectResult Compile(CastExpression cast)
        {
            var expression = Compiler.CompileNode(cast.Expression).Unwrap();

            var targetType = Compiler.CompileType(cast.TargetType).Unwrap();

            if (targetType is not Objects.Nullable)
            {
                Compiler.LogError("Casting to non-nullable type is not supported yet", cast);

                targetType = new Objects.Nullable(targetType);
            }

            if (
                Compiler.Compiler.CG.Cast(expression, targetType)
                .When(out var typeCast)
                .Error(out var error)
            )
                return Compiler.CompilationError(error, cast);

            if (
                Compiler.Compiler.IR.CompileCode(typeCast!.Cast)
                .When(out var castCode)
                .Error(out error)
            )
                return Compiler.CompilationError(error, cast);

            if (typeCast.CanFail)
                castCode!.Instructions.Add(typeCast.OnFail);

            castCode!.Types[0] = (targetType as Objects.Nullable)!.UnderlyingType;

            return ObjectResult.Ok(new Objects.RawCode(castCode));
        }

        private ObjectResult Compile(IdentifierExpression identifier)
        {
            CompilerObject? result = null;

            Compiler.Compiler.CurrentContext.PerformOperation<IScopeContext>(
                scope =>
                {
                    return scope.Get(identifier.Name, out result);
                }
            );

            if (result is not null) return ObjectResult.Ok(result);

            return Compiler.CompilationError($"Could not resolve name {identifier.Name}", identifier);
        }

        private ObjectResult Compile(IndexExpression index)
        {
            var indexable = Compiler.Compiler.Evaluate(Compiler.CompileNode(index.Target).Unwrap());

            var args = index.Arguments.Select(arg => new Argument_NEW<CompilerObject>(arg.Name, Compiler.CompileNode(arg.Value).Unwrap()));

            return ObjectResult.Ok(Compiler.Compiler.Map(indexable, @object => Compiler.Compiler.CG.Index(@object, [.. args]).Unwrap()));
        }

        private ObjectResult Compile(IsOfExpression isOf)
        {
            var value = Compiler.CompileNode(isOf.Expression).Unwrap();
            var type = Compiler.CompileType(isOf.OfType).Unwrap();

            if (
                Compiler.Compiler.CG.TypeMatch(value, type)
                .When(out var match)
                .Error(out var error)
            )
                return Compiler.CompilationError(error, isOf);

            if (
                Compiler.Compiler.IR.CompileCode(match!.Match)
                .When(out var matchCode)
                .Error(out error)
            )
                return Compiler.CompilationError(error, isOf);

            IR.VM.Nop noMatch = new();

            matchCode!.Instructions.AddRange([
                new IR.VM.Pop(),
                new IR.VM.PutFalse(),
                new IR.VM.Jump(noMatch),
            ]);

            if (isOf.Name is not null && isOf.Name != string.Empty && isOf.Name != "_")
            {
                var allocator = Compiler.Compiler.CurrentContext.FindContext<IMemoryAllocator>();
                if (allocator is null)
                    return Compiler.CompilationError($"Could not find memory allocator in context chain", isOf);

                var local = allocator.Allocate(
                    isOf.Name,
                    type,
                    new Objects.RawCode(new([
                        match.OnMatch
                    ])
                    {
                        Types = [type]
                    })
                );

                if (local is not Objects.Local localObject)
                    throw new NotImplementedException();

                Compiler.Context.CurrentScope.Set(isOf.Name, local);

                matchCode.Instructions.AddRange([
                    .. localObject.IR!.Initializer!,
                    new IR.VM.Pop(),
                    new IR.VM.PutTrue(),
                ]);
            }

            matchCode.Instructions.Add(noMatch);

            matchCode.Types.Clear();
            matchCode.Types.Add(Compiler.Compiler.TypeSystem.Boolean);

            return ObjectResult.Ok(new Objects.RawCode(matchCode));
        }

        private ObjectResult Compile(WhileExpression<Expression> @while)
        {           
            return ObjectResult.Ok(new WhileExpressionCompiler(Compiler, @while, null!).Compile());
        }
    }
}
