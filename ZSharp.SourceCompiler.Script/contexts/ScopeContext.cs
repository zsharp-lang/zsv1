using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.SourceCompiler.Script
{
    public sealed class ScopeContext()
        : ZSharp.Compiler.IContext
        , IScopeContext
    {
        private readonly Mapping<string, CompilerObject> scope = [];

        public ZSharp.Compiler.IContext? Parent { get; set; }

        public IResult Add(string name, CompilerObject value)
        {
            if (scope.ContainsKey(name))
                return Result<CompilerObject>.Error($"Name '{name}' is already defined in this scope.");

            return Set(name, value);
        }

        public IResult Get(string name)
            => scope.TryGetValue(name, out var value)
                ? Result<CompilerObject>.Ok(value)
                : Result<CompilerObject>.Error($"Name '{name}' is not defined in this scope.");

        public IResult Set(string name, CompilerObject value)
        {
            scope[name] = value;

            return Result<CompilerObject>.Ok(value);
        }
    }
}
