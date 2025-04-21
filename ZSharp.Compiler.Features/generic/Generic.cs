using CommonZ.Utils;
using ZSharp.Objects;

namespace ZSharp.Compiler
{
    public sealed class Generic(Compiler compiler) : Feature(compiler)
    {
        public bool IsGenericDefinition(CompilerObject @object)
        {
            throw new NotImplementedException();
        }

        public bool IsGenericInstance(CompilerObject @object)
        {
            throw new NotImplementedException();
        }

        public Mapping<CompilerObject, CompilerObject> GetGenericArguments(CompilerObject @object)
        {
            throw new NotImplementedException();
        }

        public CompilerObject GetGenericDefinition(CompilerObject @object)
        {
            throw new NotImplementedException();
        }

        public IGenericParameter[] GetGenericParameters(CompilerObject @object)
        {
            throw new NotImplementedException();
        }
    }
}
