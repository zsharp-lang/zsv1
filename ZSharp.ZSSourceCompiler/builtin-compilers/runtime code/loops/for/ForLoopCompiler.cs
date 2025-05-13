
namespace ZSharp.ZSSourceCompiler
{
    public class ForStatementCompiler(ZSSourceCompiler compiler, ForStatement node)
        : ContextCompiler<ForStatement, ForLoop>(compiler, node, new())
        , IOverrideCompileStatement
    {
        private CompilerObject Iterator { get; set; }

        private CompilerObject MoveNext { get; set; }

        private CompilerObject Current { get; set; }

        public override ForLoop Compile()
        {
            var source = Object.Source = Compiler.CompileNode(Node.Source);

            Iterator = Compiler.Compiler.Member(source, "getIterator");
            Iterator = Compiler.Compiler.Call(Iterator, []);
            if (!Compiler.Compiler.TypeSystem.IsTyped(Iterator, out var iteratorType))
            {
                Compiler.LogError($"For loop source must provide a valid iterator", Node);
                return Object;
            }

            MoveNext = Compiler.Compiler.Member(new Objects.RawCode(new([
                new IR.VM.Dup()
                ])
            {
                Types = [iteratorType]
            }), "moveNext");
            MoveNext = Compiler.Compiler.Call(MoveNext, []);
            MoveNext = Compiler.Compiler.Cast(MoveNext, Compiler.Compiler.TypeSystem.Boolean);

            Current = Compiler.Compiler.Member(new Objects.RawCode(new([
                new IR.VM.Dup()
                ])
            {
                Types = [iteratorType]
            }), "getCurrent");
            Current = Compiler.Compiler.Call(Current, []);

            Object.Iterator = Iterator;
            Object.MoveNext = MoveNext;
            Object.Current = Current;

            using (Context.Compiler(this))
            using (Context.Scope())
            {
                CompileForValue();
                Object.For = Compiler.CompileNode(Node.Body);
            }

            if (Node.Else is not null)
                using (Context.Scope())
                    Object.Else = CompileElse();

            return base.Compile();
        }

        protected CompilerObject CompileBreak(BreakStatement @break)
        {
            // TODO: add support for 'break from' statement

            if (@break.Value is not null)
                Compiler.LogError("Break statement in a for statement must not have a value", @break);

            return new Objects.RawCode(
                new([
                    new IR.VM.Jump(Object.EndLabel)
                ])
            );
        }

        protected CompilerObject CompileElse()
            => Compiler.CompileNode(Node.Else!);

        public CompilerObject? CompileNode(ZSSourceCompiler compiler, Statement node)
            => node switch
            {
                BreakStatement @break => CompileBreak(@break),
                _ => null
            };

        private void CompileForValue()
        {
            Object.Value = Node.Value switch {
                LetForValue let => CompileLetForValue(let),
                _ => throw new()
            };
        }

        private CompilerObject CompileLetForValue(LetForValue let)
        {
            var allocator = Compiler.Compiler.CurrentContext.FindContext<IMemoryAllocator>();
            if (allocator is null)
            {
                Compiler.LogError($"Could not find memory allocator in context chain", Node);

                throw new();
            }

            var local = allocator.Allocate(
                let.Name,
                let.Type is not null
                    ? Compiler.CompileType(let.Type)
                    : Compiler.Compiler.TypeSystem.IsTyped(Current, out var type)
                    ? type
                    : throw new(),
                Current
            );

            if (local is not Objects.Local local1)
                throw new NotImplementedException();

            Compiler.Context.CurrentScope.Set(let.Name, local);

            return new Objects.RawCode(new(local1.IR?.Initializer!)
            {
                Types = [local1.Type]
            });
        }
    }
}
