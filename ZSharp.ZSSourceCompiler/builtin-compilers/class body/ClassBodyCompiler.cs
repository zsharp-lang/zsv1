using ZSharp.Objects;

namespace ZSharp.ZSSourceCompiler
{
    public sealed class ClassBodyCompiler(ZSSourceCompiler compiler, OOPDefinition oop, GenericClass @class)
        : CompilerBase(compiler)
        , IMultipassCompiler
    {
        private List<Action> currentPass = [], nextPass = [];

        public OOPDefinition Node { get; } = oop;

        public GenericClass Class { get; } = @class;

        public void AddToNextPass(Action action)
            => nextPass.Add(action);

        public void Compile()
        {
            using (Context.Compiler(this))
                CompileContent();
        }

        private void CompileContent()
        {
            if (Node.Content is null)
                return;

            currentPass = Node.Content.Statements.Select(Compile).ToList();

            while (currentPass.Count > 0)
            {
                foreach (var item in currentPass)
                    item();

                (currentPass, nextPass) = (nextPass, currentPass);
                nextPass.Clear();
            }
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
            var compiler = new ConstructorCompiler(Compiler, constructor);

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
            var compiler = new MethodCompiler(Compiler, function, Class);

            Class.Content.Add(compiler.Object);

            if (function.Name != string.Empty)
            {
                if (!Context.CurrentScope.Get(function.Name, out var result, lookupParent: false))
                    Context.CurrentScope.Set(function.Name, result = new OverloadGroup(function.Name));
                if (result is not OverloadGroup group)
                    throw new();
                group.Overloads.Add(compiler.Object);

                if (!Class.Members.TryGetValue(function.Name, out result))
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
                field.Initializer = Compiler.CompileNode(let.Value);

                if (let.Type is not null)
                    field.Type = Compiler.CompileType(let.Type);
                else if (Compiler.Compiler.TypeSystem.IsTyped(field.Initializer, out var type))
                    field.Type = type;

                if (field.Type is null) throw new(); // TODO: Throw a proper exception of CouldNotInferType

                field.IR = Compiler.Compiler.CompileIRObject<IR.Field, IR.Class>(field, null);

                if (field.Initializer is not null)
                    field.Initializer = Compiler.Compiler.Cast(field.Initializer, field.Type);
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
                if (var.Value is not null)
                    field.Initializer = Compiler.CompileNode(var.Value);

                if (var.Type is not null)
                    field.Type = Compiler.CompileType(var.Type);
                else if (field.Initializer is not null)
                    if (Compiler.Compiler.TypeSystem.IsTyped(field.Initializer, out var type))
                        field.Type = type;

                if (field.Type is null) throw new(); // TODO: Throw a proper exception of CouldNotInferType

                field.IR = Compiler.Compiler.CompileIRObject<IR.Field, IR.Class>(field, null);

                if (field.Initializer is not null)
                    field.Initializer = Compiler.Compiler.Cast(field.Initializer, field.Type);
            };
        }
    }
}
