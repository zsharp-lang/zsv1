namespace ZSharp.RAST
{
    public sealed class RDecorated : RDefinition
    {
        public RExpression Decorator { get; set; }

        public RDefinition Decorated { get; set; }

        public RDecorated(RExpression decorator, RDefinition decorated)
            : base(decorated.Id)
        {
            Decorator = decorator;
            Decorated = decorated;
        }
    }
}
