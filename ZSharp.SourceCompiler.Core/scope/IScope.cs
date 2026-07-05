using System.Diagnostics.CodeAnalysis;

namespace ZSharp.SourceCompiler
{
    public interface IScope
    {
        public bool Add(string name, CompilerObject @object)
            => Add(name, @object, out var _);

        public bool Add(string name, CompilerObject @object, [NotNullWhen(false)] out Error? error);

        public IResult Get(string name)
            => Get(name, out var @object, out var error)
            ? Result<CompilerObject>.Ok(@object)
            : Result<CompilerObject>.Error(error);

        public bool Get(string name, [NotNullWhen(true)] out CompilerObject? @object)
            => Get(name, out @object, out var _);

        public bool Get(
            string name, 
            [NotNullWhen(true)] out CompilerObject? @object, 
            [NotNullWhen(false)] out Error? error
        );

        public bool Set(string name, CompilerObject @object)
            => Set(name, @object, out var _);

        public bool Set(string name, CompilerObject @object, [NotNullWhen(false)] out Error? error);
    }
}
