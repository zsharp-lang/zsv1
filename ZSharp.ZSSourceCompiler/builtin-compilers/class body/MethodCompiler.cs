namespace ZSharp.ZSSourceCompiler
{
    public sealed class MethodCompiler(ClassBodyCompiler compiler, Function node, CompilerObject owner, Compiler.IType self)
        : ContextCompiler<Function, Objects.Method>(compiler.Compiler, node, new(node.Name)
        {
            Owner = owner
        })
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
                //Object.Signature.VarArgs = Compile(Node.Signature.VarArgs);
                throw new NotImplementedException();

            if (Node.Signature.KwArgs is not null)
                foreach (var parameter in Node.Signature.KwArgs)
                    Object.Signature.KwArgs.Add(Compile(parameter));

            if (Node.Signature.VarKwArgs is not null)
                //Object.Signature.VarKwArgs = Compile(Node.Signature.VarKwArgs);
                throw new NotImplementedException();

            if (Node.ReturnType is not null)
            if (
                Compiler.CompileType(Node.ReturnType)
                .When(out var returnType)
                .Error(out var error)
            )
            {
                return;
            }
            else Object.ReturnType = returnType;
            else
            {
                return;
            }

                (This = Object.Signature.Args[0]).Type ??= self;

            //Object.IR = Compiler.Compiler.CompileIRObject<IR.Method, IR.Class>(Object, null);

            compiler.AddToNextPass(() =>
            {
                using (Context.Compiler(this))
                using (Context.Scope(Object))
                using (Context.Scope())
                    CompileMethodBody();
            });
        }

        private Objects.Parameter Compile(Parameter parameter)
        {
            var result = new Objects.Parameter(parameter.Name);

            Context.CurrentScope.Set(parameter.Alias ?? parameter.Name, result);

            if (
                parameter.Type is not null &&
                Compiler.CompileType(parameter.Type).Ok(out var type)
            )
                result.Type = type;

            if (parameter.Initializer is not null)
                Compiler.LogError("Parameter initializers are not supported yet.", parameter);

            return result;
        }

        private void CompileMethodBody()
        {
            if (
                Node.Body is not null &&
                Compiler.CompileNode(Node.Body).Ok(out var body)
            )
                Object.Body = body;

            Object.IR = Compiler.Compiler.CompileIRObject<IR.Method, IR.Class>(Object, null);
        }

        public ObjectResult? CompileNode(ZSSourceCompiler compiler, Statement node)
        {
            if (node is not Return @return)
                return null;

            if (@return.Value is null)
                return ObjectResult.Ok(new Objects.RawCode(new([
                    new IR.VM.Return()
                ])));

            var valueResult = Compiler.CompileNode(@return.Value);

            if (
                valueResult
                .When(out var returnValue)
                .IsError
            )
                return valueResult;

            if (
                Compiler.Compiler.IR.CompileCode(returnValue!)
                .When(out var valueCode)
                .Error(out var error)
                )
                return Compiler.CompilationError(error, @return.Value);

            valueCode!.Instructions.Add(new IR.VM.Return());
            valueCode.RequireValueType();
            valueCode.Types.Clear();

            return ObjectResult.Ok(new Objects.RawCode(valueCode));
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
                Initializer = Compiler.CompileNode(let.Value).Unwrap(),
            };

            Context.CurrentScope.Set(let.Name, local);

            local.Type = let.Type is not null
                ? Compiler.CompileType(let.Type).Unwrap()
                : Compiler.Compiler.TypeSystem.IsTyped(local.Initializer, out var type)
                ? type
                : null;

            if (local.Type is null)
                Compiler.LogError("Could not infer type of local variable.", let);

            local.IR = Compiler.Compiler.CompileIRObject<IR.VM.Local, IR.VM.FunctionBody>(local, Object.IR!.Body);

            if (local.IR.Initializer is not null)
                return new Objects.RawCode(new(local.IR.Initializer)
                {
                    Types = [local.Type]
                });

            return new Objects.RawCode(new());
        }

        private CompilerObject CompileNode(VarExpression var)
        {
            var local = new Objects.Local
            {
                Name = var.Name,
                Initializer = var.Value is null ? null : Compiler.CompileNode(var.Value).Unwrap(),
            };

            Context.CurrentScope.Set(var.Name, local);

            local.Type = var.Type is not null
                ? Compiler.CompileType(var.Type).Unwrap()
                : local.Initializer is null
                ? null
                : Compiler.Compiler.TypeSystem.IsTyped(local.Initializer, out var type)
                ? type
                : null;

            if (local.Type is null)
                Compiler.LogError("Could not infer type of local variable.", var);

            local.IR = Compiler.Compiler.CompileIRObject<IR.VM.Local, IR.VM.FunctionBody>(local, Object.IR!.Body);

            if (local.IR.Initializer is not null)
                return new Objects.RawCode(new(local.IR.Initializer)
                {
                    Types = [local.Type]
                });

            return new Objects.RawCode(new());
        }
    }
}
