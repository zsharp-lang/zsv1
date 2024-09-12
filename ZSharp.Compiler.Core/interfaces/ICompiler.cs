using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Compiler
{
    public interface ICompiler
    {
        public Code CompileIRCode(CGObject @object);

        public Code Assign(Code irCode, Assignment assignment)
            => throw new NotImplementedException();

        public Assignment AssignTo(CGObject source, CGObject target)
            => throw new NotImplementedException();

        public bool IsLiteral<T>(CGObject @object, [NotNullWhen(true)] out T? value) where T : class;

        public bool IsCTValue<T>(CGObject @object, [NotNullWhen(true)] out T? value) where T : class;
    }
}
