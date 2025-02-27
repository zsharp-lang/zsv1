using CommonZ.Utils;

namespace ZSharp.ZSSourceCompiler
{
    public sealed class ModuleCompiler(ZSSourceCompiler compiler, Module node)
        : ContextCompiler<Module, Objects.Module>(compiler, node, new(node.Name))
        , IMultipassCompiler
    {
        private List<Action> currentPass = [], nextPass = [];

        public readonly Mapping<string, Func<ZSSourceCompiler, OOPDefinition, ContextCompiler>> oopTypesCompilers = [];

        public required Objects.ClassMetaClass DefaultMetaClass { get; set; }

        public void AddToNextPass(Action action)
            => nextPass.Add(action);

        public override Objects.Module Compile()
        {
            using (Context.Compiler(this))
            using (Context.Scope(Object))
                CompileModule();

            return base.Compile();
        }

        private void CompileModule()
        {
            currentPass = Node.Body.Select(Compile).ToList();

            while (currentPass.Count > 0)
            {
                foreach (var item in currentPass)
                    item();

                (currentPass, nextPass) = (nextPass, currentPass);
                nextPass.Clear();
            }

            Compiler.Compiler.CompileIRObject<IR.Module, IR.Module>(Object, null);
        }

        private Action Compile(Statement statement)
            => statement switch
            {
                ExpressionStatement expression => Compile(expression),
                _ => throw new() // TODO: Throw a proper exception of UnknownNodeType
            };

        private Action Compile(ExpressionStatement expression)
            => expression.Expression switch
            {
                Function function => Compile(function),
                LetExpression let => Compile(let),
                Module module => Compile(module),
                OOPDefinition oop => Compile(oop),
                VarExpression var => Compile(var),
                _ => throw new() // TODO: Throw a proper exception of UnknownNodeType
            };

        private Action Compile(Function function)
        {
            var compiler = new FunctionCompiler(Compiler, function);

            Object.Content.Add(compiler.Object);

            if (function.Name != string.Empty)
            {
                Context.CurrentScope.Set(function.Name, compiler.Object);
                Object.Members.Add(function.Name, compiler.Object);
            }

            return () => compiler.Compile();
        }

        private Action Compile(LetExpression let)
        {
            var global = new Objects.Global(let.Name);

            Object.Content.Add(global);

            Context.CurrentScope.Set(let.Name, global);
            Object.Members.Add(let.Name, global);

            return () =>
            {
                global.Initializer = Compiler.CompileNode(let.Value);

                if (let.Type is not null)
                    global.Type = Compiler.CompileType(let.Type);
                else if (Compiler.Compiler.TypeSystem.IsTyped(global.Initializer, out var type))
                    global.Type = type;
                
                if (global.Type is null) throw new(); // TODO: Throw a proper exception of CouldNotInferType

                global.IR = Compiler.Compiler.CompileIRObject<IR.Global, IR.Module>(global, null);

                if (global.Initializer is not null)
                    global.Initializer = Compiler.Compiler.Cast(global.Initializer, global.Type);
            };
        }

        private Action Compile(Module module)
        {
            var compiler = new ModuleCompiler(Compiler, module)
            {
                DefaultMetaClass = DefaultMetaClass,
            };

            Object.Content.Add(compiler.Object);

            foreach (var (key, value) in oopTypesCompilers)
                compiler.oopTypesCompilers[key] = value;

            if (module.Name != string.Empty)
            {
                Context.CurrentScope.Set(module.Name, compiler.Object);
                Object.Members.Add(module.Name, compiler.Object);
            }

            return () => compiler.Compile();
        }

        private Action Compile(OOPDefinition oop)
        {
            var compiler = oopTypesCompilers[oop.Type](Compiler, oop);

            Object.Content.Add(compiler.ContextObject);

            if (oop.Name != string.Empty)
            {
                Context.CurrentScope.Set(oop.Name, compiler.ContextObject);
                Object.Members.Add(oop.Name, compiler.ContextObject);
            }

            return () => compiler.CompileNode();
        }

        private Action Compile(VarExpression var)
        {
            var global = new Objects.Global(var.Name);

            Object.Content.Add(global);

            Context.CurrentScope.Set(var.Name, global);
            Object.Members.Add(var.Name, global);

            return () =>
            {
                if (var.Value is not null)
                    global.Initializer = Compiler.CompileNode(var.Value);

                if (var.Type is not null)
                    global.Type = Compiler.CompileType(var.Type);
                else if (global.Initializer is not null)
                    if (Compiler.Compiler.TypeSystem.IsTyped(global.Initializer, out var type))
                        global.Type = type;

                if (global.Type is null) throw new(); // TODO: Throw a proper exception of CouldNotInferType

                global.IR = Compiler.Compiler.CompileIRObject<IR.Global, IR.Module>(global, null);

                if (global.Initializer is not null)
                    global.Initializer = Compiler.Compiler.Cast(global.Initializer, global.Type);
            };
        }
    }
}
