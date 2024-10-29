using System.Diagnostics.CodeAnalysis;
using ZSharp.CGObjects;

namespace ZSharp.Compiler
{
    public sealed partial class Compiler
        : ICompiler
    {
        public bool IsLiteral(CGObject @object)
            => @object is Literal;

        public bool IsLiteral(CGObject @object, [NotNullWhen(true)] out Literal? result)
            => (@object is Literal literal 
            ? (result = literal) 
            : (result = null)
            ) is not null;

        public bool IsLiteral<T>(CGObject @object, [NotNullWhen(true)] out T? value)
            where T : struct
        {
            value = default;

            if (!IsLiteral(@object, out Literal? literal)) return false;

            if (literal.Value is not T literalValue) return false;

            value = literalValue;

            return true;
        }

        public bool IsLiteral<T>(CGObject @object, [NotNullWhen(true)] out T? value)
            where T : class
        {
            if (!IsLiteral(@object, out Literal? literal)) return (value = null) is not null;

            if (literal.Value is not T literalValue) return (value = null) is not null;

            return (value = literalValue) is not null;
        }

        /// <summary>
        /// Checks if the given object is a literal string and unpacks it.
        /// </summary>
        /// <param name="object"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsString(CGObject @object, out string value)
        {
            throw new NotImplementedException();
        }
    }
}
