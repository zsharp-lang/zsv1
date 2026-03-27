namespace ZSharp.SourceCompiler.HIR.BuiltIn
{
    public class Method
        : Node
    {
        public required Function UnderlyingFunction { get; init; }

        public string Name
        {
            get => UnderlyingFunction.Name;
            set => UnderlyingFunction.Name = value;
        }

        public required ClassMemberBinding Binding { get; set; }
    }
}
