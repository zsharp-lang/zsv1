namespace ZSharp.Compiler
{
    internal partial class IRGenerator
    {
        public IRCode Read(CGObject @object)
        {
            if (@object is ICTReadable ctReadable)
                return ctReadable.Read(this);

            throw new NotImplementedException();
        }
    }
}
