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
            CGCode result = [CG.Get("import")]; // TODO: CG.Object(Config.Import);

            result.AddRange(Context.Compile(import.Source));
            result.Add(CG.Argument());

            if (import.Arguments is not null)
                foreach (var argument in import.Arguments)
                {
                    result.AddRange(Context.Compile(argument.Value));
                    result.Add(CG.Argument(argument.Name));
                }

            if (import.As is not null)
                result.AddRange([ CG.Dup(), CG.Set(import.As.Name) ]);

            if (import.Targets is not null)
                foreach (var target in import.Targets)
                    result.AddRange([
                        CG.Dup(), 
                        CG.GetMember(target.Name!), 
                        CG.Set(target.Alias?.Name ?? target.Name!)
                    ]);
        }
    }
}
