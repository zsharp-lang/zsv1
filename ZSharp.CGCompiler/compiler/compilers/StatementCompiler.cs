using ZSharp.CGRuntime;
using ZSharp.RAST;

namespace ZSharp.CGCompiler
{
    internal sealed class StatementCompiler(Context context)
        : CompilerBase(context)
    {
        internal bool Compile(RStatement statement)
        {
            switch(statement)
            {
                case RBlock block: Compile(block); break;
                case RExpressionStatement expressionStatement: Compile(expressionStatement); break;
                case RImport import: Compile(import); break;
                default: return false;
            };

            return true;
        }

        private void Compile(RBlock block)
        {
            foreach (var statement in block.Statements)
                Context.Compile(statement);
        }

        private void Compile(RExpressionStatement expressionStatement)
        {
            var code = Context.Compile(expressionStatement.Expression);

            //code.Add(CG.Object(TypeSystem.Void));
            //code.Add(CG.Cast());

            if (expressionStatement.Expression is not RDefinition)
                Emit(code);
        }

        private void Compile(RImport import)
        {
            CGCode args = [
                ..Context.Compile(import.Source),
                CG.Argument()
            ];

            if (import.Arguments is not null)
                foreach (var argument in import.Arguments)
                    args.AddRange([
                        ..Context.Compile(argument.Value),
                        CG.Argument(argument.Name)
                    ]);

            args.Add(CG.Call((import.Arguments?.Count ?? 0) + 1));

            List<CGObjects.ImportedName> importedNames = [];

            if (import.Targets is not null)
                foreach (var target in import.Targets)
                    importedNames.Add(new CGObjects.ImportedName()
                    {
                        Alias = target.Alias?.Name,
                        Name = target.Name!,
                    });

            Emit([
                CG.Object(new CGObjects.Import() {
                    Alias = import.As?.Name,
                    Arguments = args,
                    ImportedNames = importedNames,
                }),
                CG.Compile(),
            ]);
        }
    }
}
