using ZSharp.CGObjects;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    public sealed class StatementCompiler(Compiler compiler)
        : CompilerBase(compiler)
    {
        public CGObject Compile(RStatement statement)
            => statement switch
            {
                RBlock block => Compile(block),
                RExpressionStatement expressionStatement => Compile(expressionStatement),
                RImport import => Compile(import),
                _ => throw new NotImplementedException(),
            };

        private CGObject Compile(RBlock block)
        {
            if (block.IsScoped)
                using (Context.Scope())
                    return Compile(new RBlock(block.Statements)
                    {
                        IsScoped = false
                    });

            Code code = new();
            foreach (var statement in block.Statements)
                code.Append(Compiler.CompileIRCode(Compiler.CompileNode(statement)));

            return new RawCode(code);
        }

        private CGObject Compile(RExpressionStatement expressionStatement)
        {
            var result = Compiler.CompileNode(expressionStatement.Expression);

            if (expressionStatement.Expression is RDefinition)
                return new RawCode(Code.Empty); // TODO: implement real void-cast

            // manual void cast
            var code = Compiler.CompileIRCode(result);
            foreach (var type in code.Types)
                if (type != Compiler.RuntimeModule.TypeSystem.Void)
                    code.Instructions.Add(new IR.VM.Pop());

            code.Types.Clear();

            // TODO: implement the whole assignment/casting system and use it instead
            //result = Compiler.Cast(result, Compiler.TypeSystem.Void);

            return new RawCode(code);
        }

        private CGObject Compile(RImport import)
        {
            var source = Compiler.CompileNode(import.Source);

            List<Argument> arguments = [];
            if (import.Arguments is not null)
                foreach (var arg in import.Arguments)
                    arguments.Add(new(arg.Name, Compiler.CompileNode(arg.Value)));

            if (!Context.CurrentScope.Cache("import", out var importer))
                throw new();
            var importResult = Compiler.Call(importer, [new(source), ..arguments]);

            if (import.As is not null)
                Context.CurrentScope.Cache(import.As.Name, importResult);

            if (import.Targets is not null)
                foreach (var target in import.Targets)
                    Context.CurrentScope.Cache(
                        target.Alias?.Name ?? target.Name!,
                        Compiler.Member(importResult, target.Name!)
                    );

            return new RawCode(Code.Empty);
        }
    }
}
