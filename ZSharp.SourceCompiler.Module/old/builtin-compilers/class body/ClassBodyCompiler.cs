using ZSharp.Objects;

namespace ZSharp.ZSSourceCompiler
{
    public sealed class ClassBodyCompiler(ZSSourceCompiler compiler, OOPDefinition oop, Class @class)
        : CompilerBase(compiler)
        , IMultipassCompiler
    {
        private List<Action> currentPass = [], nextPass = [];

        public OOPDefinition Node { get; } = oop;

        public Class Class { get; } = @class;

        public void AddToNextPass(Action action)
            => nextPass.Add(action);

        public void Compile()
        {
            if (Node.Content is null)
                return;

            currentPass = [.. Node.Content.Statements.Select(Compile)];
        }

        public bool CompileSinglePass()
        {
            using (Context.Compiler(this))
                return CompileSinglePassImpl();
        }

        public void CompileUntilComplete()
        {
            using (Context.Compiler(this))
                while (CompileSinglePassImpl()) ;
        }

        private bool CompileSinglePassImpl()
        {
            foreach (var item in currentPass)
                item();

            (currentPass, nextPass) = (nextPass, currentPass);
            nextPass.Clear();

            return currentPass.Count > 0;
        }

        private Action Compile(Statement statement)
            => statement switch
            {
                ExpressionStatement expression => Compile(expression),
                _ => throw new(), // TODO: could not compile node ...
            };

        private Action Compile(ExpressionStatement expression)
            => expression.Expression switch
            {
                AST.Constructor constructor => Compile(constructor),
                AST.Function function => Compile(function),
                LetExpression let => Compile(let),
                //OOPDefinition oop => Compile(oop),
                VarExpression var => Compile(var),
                _ => throw new() // TODO: Throw a proper exception of UnknownNodeType
            };

        private Action Compile(AST.Constructor constructor)
        {
            var compiler = new ConstructorCompiler(this, constructor);

            compiler.Object.Owner = Class;

            Class.Content.Add(compiler.Object);

            if (constructor.Name is not null && constructor.Name != string.Empty)
            {
                Context.CurrentScope.Set(constructor.Name, compiler.Object);
                Class.Members.Add(constructor.Name, compiler.Object);
            } else
            {
                if (Class.Constructor is null)
                    Class.Constructor = new OverloadGroup(string.Empty);
                if (Class.Constructor is not OverloadGroup group)
                    throw new("???");

                group.Overloads.Add(compiler.Object);
            }

            return () => compiler.Compile();
        }

        private Action Compile(AST.Function function)
        {
            var compiler = new MethodCompiler(this, function, Class, Class);

            Class.Content.Add(compiler.Object);

            if (function.Name != string.Empty)
            {
                MethodOverloadGroup? group = null;
                if (!Compiler.Compiler.CurrentContext.PerformOperation<IScopeContext>(
                    scope => scope.Get(function.Name, out group)
                ))
                    Context.CurrentScope.Set(function.Name, group = new(function.Name));
                
                group!.Overloads.Add(compiler.Object);

                if (!Class.Members.TryGetValue(function.Name, out var _))
                    Class.Members.Add(function.Name, group);
            }

            return () => compiler.Compile();
        }

        private Action Compile(LetExpression let)
        {
            var field = new Field(let.Name)
            {
                IsReadOnly = true,
            };

            Class.Content.Add(field);

            Context.CurrentScope.Set(let.Name, field);
            Class.Members.Add(let.Name, field);

            return () =>
            {
                if (Compiler.CompileNode(let.Value).Ok(out var initializer))
                    field.Initializer = initializer;

                if (
                    let.Type is not null && 
                    Compiler.CompileType(let.Type).Ok(out var type)
                )
                    field.Type = type;
                else if (
                    field.Initializer is not null &&
                    Compiler.Compiler.TypeSystem.IsTyped(field.Initializer, out type)
                )
                    field.Type = type;

                if (field.Type is null)
                {
                    Compiler.LogError($"Field type could not be inferred", let);
                    return;
                }

                if (
                    Compiler.Compiler.IR.CompileDefinition<IR.Field, IR.Class>(field, null)
                    .When(ir => field.IR = ir)
                    .Else(error => Compiler.LogError(
                            $"Could not compile IR for field {let.Name}: {error}", let
                        )
                    ).IsError
                )
                    return;

                if (field.Initializer is not null &&
                    Compiler.Compiler.CG.ImplicitCast(field.Initializer, field.Type)
                    .When(init => field.Initializer = init)
                    .Else(error => Compiler.LogError(
                        $"Could not initialize field {let.Name}: {error}", let
                        )
                    ).IsError
                )
                    return;
            };
        }

        private Action Compile(VarExpression var)
        {
            var field = new Field(var.Name)
            {
                IsReadOnly = false,
            };

            Class.Content.Add(field);

            Context.CurrentScope.Set(var.Name, field);
            Class.Members.Add(var.Name, field);

            return () =>
            {
                if (
                    var.Value is not null &&
                    Compiler.CompileNode(var.Value).Ok(out var initializer)
                )
                    field.Initializer = initializer;

                if (
                    var.Type is not null &&
                    Compiler.CompileType(var.Type).Ok(out var type)
                )
                    field.Type = type;
                else if (
                    field.Initializer is not null &&
                    Compiler.Compiler.TypeSystem.IsTyped(field.Initializer, out type)
                )
                    field.Type = type;

                if (field.Type is null)
                {
                    Compiler.LogError($"Field type could not be inferred", var);
                    return;
                }

                if (
                    Compiler.Compiler.IR.CompileDefinition<IR.Field, IR.Class>(field, null)
                    .When(ir => field.IR = ir)
                    .Else(error => Compiler.LogError(
                            $"Could not compile IR for field {var.Name}: {error}", var
                        )
                    ).IsError
                )
                    return;

                if (field.Initializer is not null &&
                    Compiler.Compiler.CG.ImplicitCast(field.Initializer, field.Type)
                    .When(init => field.Initializer = init)
                    .Else(error => Compiler.LogError(
                        $"Could not initialize field {var.Name}: {error}", var
                        )
                    ).IsError
                )
                    return;
            };
        }
    }
}
