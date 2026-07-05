using CommonZ;
using Package.ZSharp;
using ZSharp.AST;
using ZSharp.Compiler;

namespace ZSharp.SourceCompiler.Script
{
    partial class ScriptCompiler
    {
        private void Compile(ImportStatement import)
        {
            // to compile an import statement we need to
            // generate code that:
            // 1) evaluates the source
            // 2) evaluates the arguments
            // 3) calls the import function
            // 4) constructs a new Import HIR object with the result (which should be some ISomething)
            // 5) if there's an alias, create a variable for the Import HIR and initialize it
            // 6) for each name, construct an ImportedName object and add it to the HIR node
            // 7) return: Import HIR object

            // Now, since we're doing script mode we can actually do it all manually here

            if (Context.ImportSystem.ImportFunction is null)
            {
                Interpreter.Log.Error(
                    $"Import system is not configured.",
                    new NodeLogOrigin(import)
                );
                return;
            }

            IEnumerable<CallArgument> arguments = [
                new CallArgument() {
                    Value = import.Source
                },
                .. import.Arguments ?? []
            ];

            var argumentsResults = arguments
                .Select<CallArgument, IResult<Argument, Error>>((arg, i) =>
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

            if (
                Interpreter.Evaluate(result!)
                .When(out var sourceObject)
                .Error(out error)
            )
            {
                Interpreter.Log.Error(
                    $"Failed to evaluate import: {error}",
                    new NodeLogOrigin(import)
                );
                return;
            }

            if (sourceObject is not CompilerObject importSourceCO)
            {

            ZSharp.HIR.Expression? source = null;
            if (sourceObject is not ZSharp.HIR.Expression source2)
            {
                Interpreter.Log.Error(
                    $"Import call did not evaluate to a valid object",
                    new NodeLogOrigin(import)
                );
            }
            else source = source2;

            var importHIR = new Import(source!);

            // TODO: implement scoping as a semantic API + provider rather than a context thing.

            if (import.Alias is not null)
            {
                if (Interpreter
                    .Compiler
                    .CurrentContext
                    .PerformOperation<IScopeContext>(
                        scope => !scope.Add(import.Alias, result!).Error(out error)
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
                foreach (var importedName in import.ImportedNames)
                    importHIR.ImportedNames.Add(new(importedName.Name, importedName.Alias));

            // now we have a constructed import HIR object we need to load it into the compilation context
            // the above code can also be thought of as compilation+execution/evaluation.
            // that is, all we can ideally access here is the import HIR

            // so, we need to load the document into a CO
            // then we need to get all the relevant members by name
            // then we need to add each member into the scope

            if (
                Core.Runtime.ModuleScope.RTLoader.Load(importHIR.Source)
                .When(out importSourceCO)
                .Error(out error)
            )
            {
                Interpreter.Log.Error(
                    $"Failed to load CO from import source {importHIR.Source}",
                    new NodeLogOrigin(import)
                );
                return;
            }

            }

            foreach (var importedMember in import.ImportedNames ?? [])
            {
                if (
                    Interpreter.Compiler.CG.Member(importSourceCO!, importedMember.Name)
                    .When(out var memberCO)
                    .Error(out error)
                )
                {
                    Interpreter.Log.Error(
                        $"Failed to resolve member {importedMember.Name}",
                        new NodeLogOrigin(import)
                    );
                    continue;
                }

                if (
                    !Context.CurrentScope.Add(importedMember.Name, memberCO!)
                )
                {
                    Interpreter.Log.Error(
                        $"No scope context found to import into.",
                        new NodeLogOrigin(import)
                    );
                    continue;
                }
            }
        }
    }
}
