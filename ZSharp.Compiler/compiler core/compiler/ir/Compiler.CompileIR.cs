using ZSharp.CGObjects;
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

        public IR.IRObject CompileIRObject(CGObject @object)
        {
            if (@object is Class @class) return @class.IR ?? throw new();

            throw new NotImplementedException();
        }

        public IR.IRObject CompileIRObject(ZSObject @object)
        {
            if (@object is ZSIRObject irObject) return irObject.IR;

            throw new NotImplementedException();
        }

        #endregion

        #region IR Type

        public IRType CompileIRType(CGObject @object)
        {
            if (@object is RawType rawType)
                return rawType.AsType(this);

            return CompileIRType(Evaluate(@object));

            throw new NotImplementedException();
        }

        public IRType CompileIRType(ZSObject @object)
        {
            if (CompileIRObject(@object) is IRType type)
                return type;

            throw new NotImplementedException();
        }

        #endregion

    }
}
