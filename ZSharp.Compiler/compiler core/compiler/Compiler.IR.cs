namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        public ZSharp.IR.RuntimeModule RuntimeModule { get; }

        public IRCode CompileIRCode(CompilerObject @object)
        {
            if (@object is ICompileIRCode irCode)
                return irCode.CompileIRCode(this);

            if (@object is ICTReadable ctReadable)
                return ctReadable.Read(this);

            throw new NotImplementedException(); // TODO: return null
        }

        public ZSharp.IR.IRDefinition CompileIRObject(CompilerObject @object)
        {
            if (@object is ICompileIRObject irObject)
                return irObject.CompileIRObject(this);

            return CompileIRObject<ZSharp.IR.IRDefinition>(@object, null);
        }

        public ZSharp.IR.IRDefinition CompileIRObject<Owner>(CompilerObject @object, Owner? owner)
            where Owner : ZSharp.IR.IRDefinition
        {
            if (@object is ICompileIRObject<Owner> irObject)
                return irObject.CompileIRObject(this, owner);

            throw new NotImplementedException(); // TODO: return null
        }

        public T CompileIRObject<T, Owner>(CompilerObject @object, Owner? owner)
            where T : ZSharp.IR.IRDefinition
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

            throw new NotImplementedException(); // TODO: return null
        }

        public T CompileIRType<T>(CompilerObject @object)
            where T : IRType
        {
            ICompileIRType<T>? irType;

            if ((irType = @object as ICompileIRType<T>) is not null)
                return irType.CompileIRType(this);

            if (@object is ICompileIRType irUntypedType)
                return (T)irUntypedType.CompileIRType(this);

            throw new NotImplementedException(); // TODO: return null
        }

        public T CompileIRReference<T>(CompilerObject @object)
        {
            if (@object is ICompileIRReference<T> irReference)
                return irReference.CompileIRReference(this);

            throw new NotImplementedException(); // TODO: return null
        }
    }
}
