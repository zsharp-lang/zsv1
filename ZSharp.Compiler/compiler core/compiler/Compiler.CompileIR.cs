using ZSharp.IR;
using ZSharp.VM;

namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        #region IR Code

        public Code CompileIRCode(CGObject @object)
        {
            if (@object is ICTReadable readable)
                return readable.Read(this);

            throw new NotImplementedException();
        }

        public Code CompileIRCode(ZSObject @object)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IR Object

        public IRObject CompileIRObject(CGObject @object)
        {
            throw new NotImplementedException();
        }

        public IRObject CompileIRObject(ZSObject @object)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IR Type

        public IType CompileIRType(CGObject @object)
        {
            if (@object is CGObjects.RawType rawType)
                return rawType.AsType(this);

            return CompileIRType(Evaluate(@object));

            throw new NotImplementedException();
        }

        public IType CompileIRType(ZSObject @object)
        {
            if (@object is ZSIRObject irObject && irObject.IR is IRType type)
                return type;

            throw new NotImplementedException();
        }

        #endregion

    }
}
