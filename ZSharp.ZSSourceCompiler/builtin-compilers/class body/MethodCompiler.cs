namespace ZSharp.ZSSourceCompiler
{
    public sealed class MethodCompiler(ZSSourceCompiler compiler, Function node, CompilerObject owner)
        : ContextCompiler<Function, Objects.Method>(compiler, node, new(node.Name))
        , IOverrideCompileStatement
    {
        public Objects.Parameter? This { get; private set; } = null;

        public override Objects.Method Compile()
        {
            using (Context.Compiler(this))
            using (Context.Scope(Object))
                CompileMethod();

            return base.Compile();
        }

        private void CompileMethod()
        {
            if (Node.Signature.Args is not null)
                foreach (var parameter in Node.Signature.Args)
                    Object.Signature.Args.Add(Compile(parameter));

            if (Node.Signature.VarArgs is not null)
                Object.Signature.VarArgs = Compile(Node.Signature.VarArgs);

            if (Node.Signature.KwArgs is not null)
                foreach (var parameter in Node.Signature.KwArgs)
                    Object.Signature.KwArgs.Add(Compile(parameter));

            if (Node.Signature.VarKwArgs is not null)
                Object.Signature.VarKwArgs = Compile(Node.Signature.VarKwArgs);

            if (Node.ReturnType is not null)
                Object.ReturnType = Compiler.CompileType(Node.ReturnType);
            else throw new(); // TODO: Implement Infer type

            (This = Object.Signature.Args[0]).Type ??= owner;

            //Object.IR = Compiler.Compiler.CompileIRObject<IR.Method, IR.Class>(Object, null);

            if (Context.ParentCompiler<IMultipassCompiler>(out var multipassCompiler))
                multipassCompiler.AddToNextPass(() =>
                {
                    using (Context.Compiler(this))
                    using (Context.Scope(Object))
                    using (Context.Scope())
                        CompileMethodBody();
                });
            else using (Context.Scope())
                    CompileMethodBody();
        }

        private Objects.Parameter Compile(Parameter parameter)
        {
            var result = new Objects.Parameter(parameter.Name);

            Context.CurrentScope.Set(parameter.Alias ?? parameter.Name, result);

            if (parameter.Type is not null)
                result.Type = Compiler.CompileType(parameter.Type);

            if (parameter.Initializer is not null)
                Compiler.LogError("Parameter initializers are not supported yet.", parameter);

            return result;
        }

        private void CompileMethodBody()
        {
            if (Node.Body is not null)
                Object.Body = Compiler.CompileNode(Node.Body);

            Object.IR = Compiler.Compiler.CompileIRObject<IR.Method, IR.Class>(Object, null);
        }

        public CompilerObject? CompileNode(ZSSourceCompiler compiler, Statement node)
        {
            if (node is not Return @return)
                return null;

            if (@return.Value is null)
                return new Objects.RawCode(new([
                    new IR.VM.Return()
                ]));

            var valueObject = Compiler.CompileNode(@return.Value);
            var valueCode = Compiler.Compiler.CompileIRCode(valueObject);

            if (valueCode is null)
            {
                Compiler.LogError("Return expression could not be compiled.", node);
                return null;
            }

            valueCode.Instructions.Add(new IR.VM.Return());
            valueCode.Types.Clear();

            return new Objects.RawCode(valueCode);
        }

        public CompilerObject? CompileNode(ZSSourceCompiler compiler, Expression node)
            => node switch
            {
                LetExpression let => CompileNode(let),
                VarExpression var => CompileNode(var),
                _ => null
            };

        private CompilerObject CompileNode(LetExpression let)
        {
            var local = new Objects.Local
            {
                Name = let.Name,
                Initializer = Compiler.CompileNode(let.Value),
            };

            Context.CurrentScope.Set(let.Name, local);

            local.Type = let.Type is not null
                ? Compiler.CompileNode(let.Type)
                : Compiler.Compiler.TypeSystem.IsTyped(local.Initializer, out var type)
                ? type
                : null;

            if (local.Type is null)
                Compiler.LogError("Could not infer type of local variable.", let);

            local.IR = Compiler.Compiler.CompileIRObject<IR.VM.Local, IR.VM.MethodBody>(local, Object.IR!.Body);

            var code = Compiler.Compiler.CompileIRCode(Compiler.Compiler.Cast(local.Initializer, local.Type!));

            code.Instructions.Add(new IR.VM.Dup());
            code.Instructions.Add(new IR.VM.SetLocal(local.IR));

            return new Objects.RawCode(code);
        }

        private CompilerObject CompileNode(VarExpression var)
        {
            var local = new Objects.Local
            {
                Name = var.Name,
                Initializer = var.Value is null ? null : Compiler.CompileNode(var.Value),
            };

            Context.CurrentScope.Set(var.Name, local);

            local.Type = var.Type is not null
                ? Compiler.CompileNode(var.Type)
                : local.Initializer is null
                ? null
                : Compiler.Compiler.TypeSystem.IsTyped(local.Initializer, out var type)
                ? type
                : null;

            if (local.Type is null)
                Compiler.LogError("Could not infer type of local variable.", var);

            local.IR = Compiler.Compiler.CompileIRObject<IR.VM.Local, IR.VM.MethodBody>(local, Object.IR!.Body);

            if (local.Initializer is not null)
            {
                var code = Compiler.Compiler.CompileIRCode(Compiler.Compiler.Cast(local.Initializer, local.Type!));

                code.Instructions.Add(new IR.VM.Dup());
                code.Instructions.Add(new IR.VM.SetLocal(local.IR));

                return new Objects.RawCode(code);
            }

            return new Objects.RawCode(new());
        }
    }
}
