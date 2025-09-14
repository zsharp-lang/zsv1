using CommonZ;
using ZSharp.AST;
using ZSharp.Compiler;

namespace ZSharp.SourceCompiler.Script
{
    partial class ScriptCompiler
    {
        private void Compile(ImportStatement import)
        {
            IEnumerable<CallArgument> arguments = [
                new CallArgument() {
                    Value = import.Source
                },
                .. import.Arguments ?? []
            ];

            var argumentsResults = arguments
                .Select((arg, i) =>
                {
                    if (
                        Compile(arg.Value)
                        .When(out var value)
                        .Error(out var error)
                    )
                    {
                        Interpreter.Log.Error(
                            $"Failed to compile import argument '{arg.Name ?? i.ToString()}': {error}",
                            new NodeLogOrigin(arg.Value)
                        );
                        return Result<Argument, Error>.Error(error);
                    }

                    return Result<Argument, Error>.Ok(
                        new(arg.Name, value!)
                    );
                })
                .ToArray();

            if (argumentsResults.Any(r => r.IsError))
                return;

            var importResult = Interpreter.Compiler.CG.Call(
                Context.ImportSystem.ImportFunction,
                [.. argumentsResults.Select(r => r.Unwrap())]
            );

            if (importResult.When(out var result).Error(out var error))
            {
                Interpreter.Log.Error(
                    $"Failed to compile import: {error}",
                    new NodeLogOrigin(import)
                );
                return;
            }

            Platform.Runtime.IEvaluationContext? evaluationContext = null;
            var codeContext = DebugContext;
            if (Interpreter.Runtime.DebugEnabled)
            {
                evaluationContext = Interpreter.Runtime.EvaluationContextFactory.CreateEvaluationContext();

                result = new Objects.ExpressionWrapper(
                    codeContext = new(
                        evaluationContext.Module.DefineDocument(
                            DocumentPath, 
                            System.Diagnostics.SymbolStore.SymLanguageType.CSharp,
                            System.Diagnostics.SymbolStore.SymLanguageVendor.Microsoft,
                            System.Diagnostics.SymbolStore.SymDocumentType.Text
                        )
                    ),
                    import.TokenInfo.ImportKeyword.Span, 
                    result!
                );
            }


            if (
                Interpreter.Evaluate(result!, evaluationContext, codeContext)
                .When(out var importObject)
                .Error(out error)
                )
            {
                Interpreter.Log.Error(
                    $"Failed to evaluate import: {error}",
                    new NodeLogOrigin(import)
                );
                return;
            }
            result = ZSharp.Interpreter.CTServices.InfoOf(importObject!);

            if (import.Alias is not null)
            {
                if (Interpreter
                    .Compiler
                    .CurrentContext
                    .PerformOperation<IScopeContext>(
                        scope => !scope.Add(import.Alias, result).Error(out error)
                    )
                )
                {
                    Interpreter.Log.Error(
                        $"No scope context found to import into.",
                        new NodeLogOrigin(import)
                    );
                    return;
                }
            }

            if (import.ImportedNames is not null)
            {
                var scope = Interpreter.Compiler.CurrentContext.FindFirstContext<IScopeContext>();
                if (scope is null)
                {
                    Interpreter.Log.Error(
                        $"No scope context found to import into.",
                        new NodeLogOrigin(import)
                    );
                    return;
                }

                var memberResults = import.ImportedNames
                    .Select(name => (name, Interpreter.Compiler.CG.Member(result, name.Name)))
                    .Select(vs =>
                    {
                        var (name, r) = vs;

                        return (name, result: r.Else(e => Interpreter.Log.Error(
                            $"Failed to import member '{name.Name}': {e}",
                            new NodeLogOrigin(name)
                        )));
                    })
                    .ToArray();

                if (memberResults.Any(vs => vs.result.IsError))
                    return;

                foreach (var (name, memberResult) in memberResults)
                    scope.Set(
                        name.Alias ?? name.Name,
                        memberResult.Unwrap()
                    );
            }
        }
    }
}
