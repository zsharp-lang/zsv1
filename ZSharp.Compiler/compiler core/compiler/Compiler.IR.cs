namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        public IR.RuntimeModule RuntimeModule { get; }

        public IRCode CompileIRCode(CompilerObject @object)
        {
            if (@object is ICompileIRCode irCode)
                return irCode.CompileIRCode(this);

            if (@object is ICTReadable ctReadable)
                return ctReadable.Read(this);

            throw new NotImplementedException(); // TODO: return null
        }

        public IR.IRObject CompileIRObject(CompilerObject @object)
        {
            if (@object is ICompileIRObject irObject)
                return irObject.CompileIRObject(this);

            return CompileIRObject<IR.IRObject>(@object, null);
        }

        public IR.IRObject CompileIRObject<Owner>(CompilerObject @object, Owner? owner)
            where Owner : IR.IRObject
        {
            if (@object is ICompileIRObject<Owner> irObject)
                return irObject.CompileIRObject(this, owner);

            throw new NotImplementedException(); // TODO: return null
        }

        public T CompileIRObject<T, Owner>(CompilerObject @object, Owner? owner)
            where T : IR.IRObject
            where Owner : class
        {
            if (@object is ICompileIRObject<T, Owner> irObject)
                return irObject.CompileIRObject(this, owner);

            throw new NotImplementedException(); // TODO: return null
        }

        public IRType CompileIRType(CompilerObject @object)
        {
            ICompileIRType? irType;

            if ((irType = @object as ICompileIRType) is not null)
                return irType.CompileIRType(this);

            @object = Evaluate(@object);

            if ((irType = @object as ICompileIRType) is not null)
                return irType.CompileIRType(this);

            throw new NotImplementedException(); // TODO: return null
        }

        public T CompileIRType<T>(CompilerObject @object)
            where T : IRType
        {
            ICompileIRType<T>? irType;

            if ((irType = @object as ICompileIRType<T>) is not null)
                return irType.CompileIRType(this);

            @object = Evaluate(@object);

            if ((irType = @object as ICompileIRType<T>) is not null)
                return irType.CompileIRType(this);

            throw new NotImplementedException(); // TODO: return null
        }
    }
}
