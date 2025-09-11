using ZSharp.Compiler;

namespace ZSharp.ZSSourceCompiler
{
    public sealed class StatementCompiler(ZSSourceCompiler compiler)
        : CompilerBase(compiler)
    {
        public ObjectResult? CompileNode(Statement statement)
            => statement switch
            {
                BlockStatement block => Compile(block),
                CaseStatement @case => Compile(@case),
                ExpressionStatement expressionStatement => Compile(expressionStatement),
                ForStatement @for => Compile(@for),
                IfStatement @if => Compile(@if),
                ImportStatement import => Compile(import),
                _ => null
            };

        private ObjectResult Compile(Expression expression)
            => expression switch
            {
                WhileExpression<Statement> @while => Compile(@while),
                _ => Compiler.CompileNode(expression)
            };

        private ObjectResult Compile(BlockStatement block)
        {
            var code = new Compiler.IRCode();

            var coResults = block.Statements
                .Select(Compiler.CompileNode)
                .ToArray();

            if (CombineErrorResults(coResults, out var coErrors))
                return ObjectResult.Error(coErrors);

            var irResults = coResults
                .Select(coResult => coResult.Unwrap())
                .Select(Compiler.Compiler.IR.CompileCode)
                .Select(irResult => 
                    irResult.Else(error => new CompilationError(null!, error) as Error)
                )
                .ToArray();

            if (CombineErrorResults(irResults, out var irErrors))
                return ObjectResult.Error(irErrors);

            foreach (var ir in irResults.Select(irResult => irResult.Unwrap()))
                code.Append(ir);

            if (!code.IsVoid)
                return Compiler.CompilationError(
                    $"Block must not have a value", block
                );

            return ObjectResult.Ok(new Objects.RawCode(code));
        }

        private ObjectResult Compile(ExpressionStatement expressionStatement)
        {
            if (expressionStatement.Expression is null)
                return ObjectResult.Ok(new Objects.RawCode(new()));

            var coResult = expressionStatement.Expression switch {
                WhileExpression<Statement> @while => Compile(@while),
                _ => Compiler.CompileNode(expressionStatement.Expression)
            };

            if (
                coResult
                .When(out var co)
                .IsError
            )
                return coResult;

            var irResult = Compiler.Compiler.IR.CompileCode(co!);

            if (
                irResult
                .When(out var ir)
                .Error(out var irError)
            )
                return Compiler.CompilationError(
                    irError, expressionStatement.Expression
                );

            if (ir!.IsArray)
                return Compiler.CompilationError(
                    "Expression evaluated to more than 1 values", 
                    expressionStatement.Expression
                );

            if (ir.IsValue)
            {
                ir.Instructions.Add(new IR.VM.Pop());
                ir.Types.Clear();
            }

            return ObjectResult.Ok(new Objects.RawCode(ir));
        }

        private ObjectResult Compile(CaseStatement @case)
        {
            var ofResult = @case.Of is null
                ? ObjectResult.Ok(Compiler.Operators.Binary.Cache("==") ?? throw new())
                : Compiler.CompileNode(@case.Of);

            var valueResult = @case.Value is null
                ? ObjectResult.Ok(Compiler.Compiler.CreateTrue())
                : Compiler.CompileNode(@case.Value);

            var elseResult = @case.Else is null
                ? ObjectResult.Ok(new Objects.RawCode(new()))
                : Compiler.CompileNode(@case.Else);

            var clausesResults = @case.WhenClauses
                .Select(clause =>
                {
                    var bodyResult = Compiler.CompileNode(clause.Body ?? throw new());
                    var valueResult = Compiler.CompileNode(clause.Value);

                    if (CombineErrorResults([bodyResult, valueResult], out var error))
                        return Result<Objects.Case.When, Error>.Error(error);

                    return Result<Objects.Case.When, Error>.Ok(new()
                    {
                        Body = bodyResult.Unwrap(),
                        Value = valueResult.Unwrap(),
                    });
                }).ToArray();

            if (CombineErrorResults([
                ofResult,
                valueResult,
                .. clausesResults.Select(r => r.When(v => v as CompilerObject))
                ], out var error))
                return ObjectResult.Error(error);

            return ObjectResult.Ok(new Objects.Case()
            {
                Of = ofResult.Unwrap(),
                Value = valueResult.Unwrap(),
                Else = elseResult.Unwrap(),
                Clauses = [.. clausesResults.Select(r => r.Unwrap())],
            });
        }

        private ObjectResult Compile(ForStatement @for)
            => ObjectResult.Ok(new ForStatementCompiler(Compiler, @for).Compile());

        private ObjectResult Compile(IfStatement @if)
        {
            var conditionResult = Compiler.CompileNode(@if.Condition);
            var bodyResult = Compiler.CompileNode(@if.If);
            var elseResult = @if.Else is null
                ? ObjectResult.Ok(new Objects.RawCode(new()))
                : Compiler.CompileNode(@if.Else);

            if (CombineErrorResults([
                conditionResult,
                bodyResult,
                elseResult,
                ], out var error))
                return ObjectResult.Error(error);

            return ObjectResult.Ok(new Objects.If()
            {
                Condition = conditionResult.Unwrap(),
                Body = bodyResult.Unwrap(),
                Else = elseResult.Unwrap()
            });
        }

        private ObjectResult Compile(ImportStatement import)
        {
            IEnumerable<CallArgument> arguments = [
                new CallArgument() {
                    Value = import.Source
                },
                .. import.Arguments ?? []
            ];

            var argumentsResults = arguments
                .Select(arg =>
                {
                    if (
                        Compiler.CompileNode(arg.Value)
                        .When(out var value)
                        .Error(out var error)
                    )
                        return Result<Argument_NEW<CompilerObject>, Error>.Error(error);

                    return Result<Argument_NEW<CompilerObject>, Error>.Ok(
                        new(arg.Name, value!)
                    );
                })
                .ToArray();

            if (CombineErrorResults(argumentsResults, out var argumentErrors))
                return ObjectResult.Error(argumentErrors);

            var importResult = Compiler.Compiler.CG.Call(
                Compiler.ImportSystem.ImportFunction,
                [.. argumentsResults.Select(r => r.Unwrap())]
            );

            if (importResult.When(out var result).Error(out var error))
                return Compiler.CompilationError(error, import);

            if (import.Alias is not null)
                Context.CurrentScope.Set(import.Alias, result!);

            if (import.ImportedNames is not null)
            {
                var memberResults = import.ImportedNames
                    .Select(name => (name, Compiler.Compiler.CG.Member(result!, name.Name)))
                    .Select(vs =>
                    {
                        var (name, r) = vs;

                        return (name, result: r.Else(e => new CompilationError(name, e) as Error));
                    })
                    .ToArray();

                if (CombineErrorResults(
                    memberResults.Select(t => t.result), 
                    out var memberErrors
                ))
                    return ObjectResult.Error(memberErrors);

                foreach (var (name, memberResult) in memberResults)
                    Context.CurrentScope.Set(
                        name.Alias ?? name.Name,
                        memberResult.Unwrap()
                    );
            }

            return ObjectResult.Ok(result!);
        }

        private ObjectResult Compile(WhileExpression<Statement> @while)
            => ObjectResult.Ok(new WhileStatementCompiler(Compiler, @while).Compile());
    }
}
