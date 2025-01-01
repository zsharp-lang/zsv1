using CommonZ.Utils;
using System.Diagnostics.CodeAnalysis;

namespace ZSharp.ZSSourceCompiler
{
    public sealed partial class Context
    {
        private readonly Stack<CompilerBase> compilerStack = [];

        public CompilerBase CurrentCompiler => compilerStack.Peek();

        public CompilerBase? ParentCompiler(int level = 0)
            => compilerStack.ElementAtOrDefault(level + 1);

        public T? ParentCompiler<T>(int level = 0)
            where T : class
            => ParentCompiler(level) as T;

        public bool ParentCompiler<T>([NotNullWhen(true)] out T? parent, int level = 0)
            where T : class
            => (parent = ParentCompiler<T>(level)) is not null;

        public T? Compiler<T>()
            where T : class
        {
            foreach (var compiler in compilerStack)
                if (compiler is T t)
                    return t;

            return null;
        }

        public bool Compiler<T>([NotNullWhen(true)] out T? compiler)
            where T : class
            => (compiler = Compiler<T>()) is not null;

        public IEnumerable<T> Compilers<T>()
            where T : class
        {
            foreach (var compiler in compilerStack)
                if (compiler is T t)
                    yield return t;
        }

        public ContextManager Compiler(CompilerBase compiler)
        {
            compilerStack.Push(compiler);

            return new(() => compilerStack.Pop());
        }
    }
}
