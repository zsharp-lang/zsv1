namespace ZSharp.Compiler
{
    internal partial class IRGenerator
    {
        /// <summary>
        /// Generates code that handles casting from type source to type target.
        /// The value (of type source) is assumed to be on the top of the stack.
        /// </summary>
        /// <param name="target">The type of the target location.</param>
        /// <param name="source">The type of the source value.</param>
        /// <returns>Code that will cast from <paramref name="source"/> 
        /// to <paramref name="target"/>.</returns>
        public Code AssignFrom(IRType target, IRType source)
            => AssignTo(source, target);

        /// <summary>
        /// Generates code that handles casting from type source to type target.
        /// The value (of type source) is assumed to be on the top of the stack.
        /// </summary>
        /// <param name="source">The type of the source value.</param>
        /// <param name="target">The type of the target location.</param>
        /// <returns>Code that will cast from <paramref name="source"/> 
        /// to <paramref name="target"/>.</returns>
        public Code AssignTo(IRType source, IRType target)
        {
            if (source == target)
                return Code.Empty;
            throw new NotImplementedException();
        }

        public Code Read(CGObject @object)
        {
            if (@object is ICTReadable ctReadable)
                return ctReadable.Read(this);

            if (@object is CGObjects.Global global)
                return new([
                    new IR.VM.GetGlobal(global.IR!)
                ])
                {
                    MaxStackSize = 1,
                    Types = [global.IR!.Type]
                };

            throw new NotImplementedException();
        }

        public Code Read(IEnumerable<CGObject> objects)
        {
            Code result = new();

            foreach (var @object in objects)
                result.Append(Read(@object));

            return result;
        }
    }
}
