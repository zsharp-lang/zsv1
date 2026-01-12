using CommonZ.Utils;

namespace ZSharp.Compiler.Features.Generic
{
    public sealed class Generic
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
