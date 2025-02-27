namespace ZSharp.ZSSourceCompiler
{
    public sealed class StatementCompiler(ZSSourceCompiler compiler)
        : CompilerBase(compiler)
    {
        public CompilerObject? CompileNode(Statement statement)
            => statement switch
            {
                BlockStatement block => Compile(block),
                CaseStatement @case => Compile(@case),
                ExpressionStatement expressionStatement => Compile(expressionStatement),
                IfStatement @if => Compile(@if),
                ImportStatement import => Compile(import),
                _ => null
            };

        private CompilerObject Compile(Expression expression)
            => expression switch
            {
                WhileExpression<Statement> @while => Compile(@while),
                _ => Compiler.CompileNode(expression)
            };

        private CompilerObject Compile(BlockStatement block)
        {
            var result = new Compiler.IRCode();

            foreach (var statement in block.Statements)
                result.Append(Compiler.Compiler.CompileIRCode(Compiler.CompileNode(statement)));

            result.RequireVoidType();

            return new Objects.RawCode(result);
        }

        private CompilerObject Compile(ExpressionStatement expressionStatement)
        {
            if (expressionStatement.Expression is null)
                return new Objects.RawCode(new());

            var result = Compiler.Compiler.CompileIRCode(Compile(expressionStatement.Expression));

            if (result.IsValue)
            {
                result.Instructions.Add(new IR.VM.Pop());
                result.Types.RemoveAt(result.Types.Count - 1);
            }

            result.RequireVoidType();

            return new Objects.RawCode(result);
        }

        private CompilerObject Compile(CaseStatement @case)
        {
            var result = new Objects.Case()
            {
                Of = @case.Of is null ? Compiler.Operators.Cache("==")! : Compiler.CompileNode(@case.Of),
                Value = @case.Value is null ? Compiler.Compiler.CreateTrue() : Compiler.CompileNode(@case.Value),
                Else = @case.Else is null ? null : Compiler.CompileNode(@case.Else),
            };

            foreach (var whenClause in @case.WhenClauses)
            {
                var clause = new Objects.Case.When()
                {
                    Body = Compiler.CompileNode(whenClause.Body ?? throw new()),
                    Value = Compiler.CompileNode(whenClause.Value)
                };

                result.Clauses.Add(clause);
            }

            return result;
        }

        private CompilerObject Compile(IfStatement @if)
        {
            var result = new Objects.If()
            {
                Body = Compiler.CompileNode(@if.If),
                Condition = Compiler.CompileNode(@if.Condition),
                Else = @if.Else is null ? null : Compiler.CompileNode(@if.Else)
            };

            return result;
        }

        private CompilerObject Compile(ImportStatement import)
        {
            if (import.Arguments is not null)
                Compiler.LogError($"Import arguments are not supported yet.", import);

            var source = Compiler.CompileNode(import.Source);

            var result = Compiler.Compiler.Call(
                Compiler.ImportSystem.ImportFunction,
                [
                    new(source),
                    .. (import.Arguments ?? []).Select(
                        arg => new Compiler.Argument(arg.Name, Compiler.CompileNode(arg.Value))
                    )
                ]
            );

            if (import.ImportedNames is not null)
                foreach (var importedName in import.ImportedNames)
                    Context.CurrentScope.Set(
                        importedName.Alias ?? importedName.Name,
                        Compiler.Compiler.Member(result, importedName.Name)
                    );

            if (import.Alias is not null)
                Context.CurrentScope.Set(import.Alias, result);

            return result;
        }

        private WhileLoop Compile(WhileExpression<Statement> @while)
            => new WhileStatementCompiler(Compiler, @while).Compile();
    }
}
