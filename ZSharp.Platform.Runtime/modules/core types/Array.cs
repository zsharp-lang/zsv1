using ZSharp.IR;

namespace ZSharp.Platform.Runtime.Modules.CoreTypes
{
    public sealed class Array(Class @class)
    {
        public TypeDefinition Definition => @class;

        public Constructor Empty { get; init; }

        public Constructor New { get; init; }

        internal Array(Module module, Class @class)
            : this(@class)
        {
            @class.GenericParameters.Add(new("ElementType"));

            @class.Constructors.Add(
                Empty = new(string.Empty)
                {
                    Method = new(module.Void.Reference)
                }
            );

            @class.Constructors.Add(
                New = new("New")
                {
                    Method = new(module.Void.Reference)
                }
            );
            New.Method.Signature.Args.Parameters.Add(new("capacity", module.Size.Reference));
        }

        public IType Reference(IType elementType)
            => new ConstructedClass(@class) { Arguments = { elementType } };
    }
}
