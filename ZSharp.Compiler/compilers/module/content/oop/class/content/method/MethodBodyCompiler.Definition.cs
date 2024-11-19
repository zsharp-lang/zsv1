using ZSharp.CGObjects;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal sealed partial class MethodBodyCompiler
    {
        protected override CGObject? CompileContextItem(RDefinition definition)
            => definition switch
            {
                RLetDefinition let => Compile(let),
                RVarDefinition var => Compile(var),
                _ => null,
            };

        private Local Compile(RLetDefinition let)
        {
            Local local = new()
            {
                Name = let.Name ?? throw new(),
                Initializer = Compiler.CompileNode(let.Value)
            };

            if (let.Type is not null)
                local.Type = Compiler.CompileNode(let.Type);

            if (Context.CurrentScope.Contains(local.Name))
                Context.CurrentScope.Uncache(local.Name);

            Context.CurrentScope.Cache(local.Name, local);

            var initializer = Compiler.CompileIRCode(local.Initializer);
            var type = Compiler.CompileIRType(
                local.Type is null
                ? initializer.RequireValueType()
                : local.Type
            );

            local.IR = new(local.Name, type)
            {
                Initializer = [.. initializer.Instructions],
                IsReadOnly = true,
            };

            Result.IR!.Body.Instructions.AddRange([
                .. initializer.Instructions,
                new IR.VM.SetLocal(local.IR),
            ]);

            Result.IR.Body.Locals.Add(local.IR);

            return local;
        }

        private Local Compile(RVarDefinition var)
        {
            Local local = new()
            {
                Name = var.Name ?? throw new(),
                Initializer = var.Value is null ? null : Compiler.CompileNode(var.Value)
            };

            if (var.Type is not null)
                local.Type = Compiler.CompileNode(var.Type);

            if (Context.CurrentScope.Contains(local.Name))
                Context.CurrentScope.Uncache(local.Name);

            Context.CurrentScope.Cache(local.Name, local);

            var initializer = local.Initializer is null 
                ? null : Compiler.CompileIRCode(local.Initializer);

            IRType type;
            if (local.Type is not null)
                type = Compiler.CompileIRType(local.Type);
            else if (initializer is not null)
                type = Compiler.CompileIRType(initializer.RequireValueType());
            else throw new();

            local.IR = new(local.Name, type)
            {
                Initializer = [.. initializer?.Instructions ?? []],
                IsReadOnly = false,
            };

            if (initializer is not null)
                Result.IR!.Body.Instructions.AddRange([
                    .. initializer.Instructions,
                    new IR.VM.SetLocal(local.IR),
                ]);

            Result.IR!.Body.Locals.Add(local.IR);

            return local;
        }
    }
}
