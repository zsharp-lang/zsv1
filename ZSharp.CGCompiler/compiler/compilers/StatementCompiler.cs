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
                case RExpressionStatement expressionStatement: Compile(expressionStatement); break;
                case RImport import: Compile(import); break;
                default: return false;
            };

            return true;
        }

        private void Compile(RExpressionStatement expressionStatement)
        {
            var code = Context.Compile(expressionStatement.Expression);

            //code.Add(CG.Object(TypeSystem.Void));
            //code.Add(CG.Cast());

            Emit(code);
        }

        private void Compile(RImport import)
        {
            Emit([
                CG.Get("import"), // TODO: CG.Object(Config.Import);
                ..Context.Compile(import.Source),
                CG.Argument()
            ]);

            if (import.Arguments is not null)
                foreach (var argument in import.Arguments)
                    Emit([
                        ..Context.Compile(argument.Value),
                        CG.Argument(argument.Name)
                    ]);

            Emit([CG.Call((import.Arguments?.Count ?? 0) + 1)]);

            if (import.As is not null)
                Emit([ 
                    CG.Dup(), 
                    CG.Set(import.As.Name) 
                ]);

            if (
                (import.Targets?.Count ?? 0) == 1
                && import.Targets![0] is RImportTarget singleTarget
            )
                Emit([
                    CG.GetMember(singleTarget.Name!),
                    CG.Set(singleTarget.Alias?.Name ?? singleTarget.Name!)
                    ]);

            else if (import.Targets is not null)
                foreach (var target in import.Targets)
                    Emit([
                        CG.Dup(), 
                        CG.GetMember(target.Name!), 
                        CG.Set(target.Alias?.Name ?? target.Name!)
                    ]);
        }
    }
}
