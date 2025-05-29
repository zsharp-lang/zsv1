using ZSharp.Compiler;

namespace ZSharp.ZSSourceCompiler
{
    public sealed partial class ExpressionCompiler(ZSSourceCompiler compiler)
        : CompilerBase(compiler)
    {
        public CompilerObject? CompileNode(Expression expression)
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

        private CompilerObject Compile(BinaryExpression binary)
        {
            // TODO: NO SPECIAL TREATMENT FOR OPERATORS . and =

            var left = Compiler.CompileNode(binary.Left);

            if (binary.Operator == ".")
            {
                CompilerObject member;

                if (binary.Right is IdentifierExpression identifier)
                    member = Compiler.Compiler.CreateString(identifier.Name);

                else if (binary.Right is not LiteralExpression literal)
                    throw new("Expected a literal expression on the right side of the dot operator.");

                else member = Compile(literal);

                if (Compiler.Compiler.IsString(member, out var memberName))
                    return Compiler.Compiler.Member(left, memberName);
                if (Compiler.Compiler.IsLiteral<int>(member, out var memberIndex))
                    return Compiler.Compiler.Member(left, memberIndex.Value);

                Compiler.LogError("Expected a string or an integer literal on the right side of the dot operator.", binary.Right);
            }

            var right = Compiler.CompileNode(binary.Right);

            if (binary.Operator == "=")
                return Compiler.Compiler.Assign(left, right);

            if (!Compiler.Operators.Binary.Cache(binary.Operator, out var @operator))
                Compiler.LogError($"Operator '{binary.Operator}' is not defined.", binary);

            return Compiler.Compiler.Call(@operator, [new(left), new(right)]);
        }

        private CompilerObject Compile(CallExpression call)
        {
            var callable = Compiler.Compiler.Evaluate(Compiler.CompileNode(call.Callee));

            var args = call.Arguments.Select(arg => new Compiler.Argument(arg.Name, Compiler.CompileNode(arg.Value)));

            return Compiler.Compiler.Call(callable, args.ToArray());
        }

        private CompilerObject Compile(CastExpression cast)
        {
            var expression = Compiler.CompileNode(cast.Expression);

            var targetType = Compiler.CompileType(cast.TargetType);

            if (targetType is not Objects.Nullable)
            {
                Compiler.LogError("Casting to non-nullable type is not supported yet", cast);

                targetType = new Objects.Nullable(targetType);
            }

            var castResult = Compiler.Compiler.CG.Cast(expression, targetType);

            TypeCast typeCast;

            if (castResult.Error(out var error))
            {
                Compiler.LogError(error, cast);

                return Compiler.Compiler.CreateNull();
            }
            else typeCast = castResult.Unwrap();

            var castCodeResult = Compiler.Compiler.IR.CompileCode(typeCast.Cast);

            IRCode castCode;

            if (castCodeResult.Error(out error))
            {
                Compiler.LogError(error, cast);

                return Compiler.Compiler.CreateNull();
            } else castCode = castCodeResult.Unwrap();

            castCode.Instructions.Add(typeCast.OnCast);
            if (typeCast.CanFail)
                castCode.Instructions.Add(typeCast.OnFail);

            castCode.Types[0] = (targetType as Objects.Nullable)!.UnderlyingType;

            return new Objects.RawCode(castCode); // TODO: add OnCast and OnFail handlers
        }

        private CompilerObject Compile(IdentifierExpression identifier)
        {
            CompilerObject? result = null;

            Compiler.Compiler.CurrentContext.PerformOperation<IScopeContext>(
                scope =>
                {
                    return scope.Get(identifier.Name, out result);
                }
            );

            if (result is not null) return result;

            Compiler.LogError($"Could not resolve name {identifier.Name}", identifier);
            return Compiler.Compiler.CreateString($"<UnresolvedName {identifier.Name}>"); // TODO: return an object that knows it doesn't exist.
        }

        private CompilerObject Compile(IndexExpression index)
        {
            var indexable = Compiler.Compiler.Evaluate(Compiler.CompileNode(index.Target));

            var args = index.Arguments.Select(arg => new Compiler.Argument(arg.Name, Compiler.CompileNode(arg.Value)));

            return Compiler.Compiler.Map(indexable, @object => Compiler.Compiler.Index(@object, [.. args]));
        }

        private CompilerObject Compile(IsOfExpression isOf)
        {
            var value = Compiler.CompileNode(isOf.Expression);
            var type = Compiler.CompileType(isOf.OfType);

            if (!Compiler.UnpackResult(
                    Compiler.Compiler.CG.TypeMatch(value, type), 
                    out var match, 
                    isOf
                )
            )
                return Compiler.Compiler.CreateFalse();

            if (!Compiler.UnpackResult(
                    Compiler.Compiler.IR.CompileCode(match.Match),
                    out var matchCode,
                    isOf
                )
            )
                return Compiler.Compiler.CreateFalse();

            IR.VM.Nop noMatch = new();

            matchCode.Instructions.AddRange([
                new IR.VM.Pop(),
                new IR.VM.PutFalse(),
                new IR.VM.Jump(noMatch),
            ]);

            if (isOf.Name is not null && isOf.Name != string.Empty && isOf.Name != "_")
            {
                var allocator = Compiler.Compiler.CurrentContext.FindContext<IMemoryAllocator>();
                if (allocator is null)
                {
                    Compiler.LogError($"Could not find memory allocator in context chain", isOf);

                    return Compiler.Compiler.CreateFalse();
                }

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

            return new Objects.RawCode(matchCode);
        }

        private WhileLoop Compile(WhileExpression<Expression> @while)
        {           
            return new WhileExpressionCompiler(Compiler, @while, null!).Compile();
        }
    }
}
