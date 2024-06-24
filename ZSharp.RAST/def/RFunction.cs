namespace ZSharp.RAST
{
    public class RFunction : RDefinition
    {
        /// <summary>
        /// Generic parameters of the function or <see cref="null"/> if the function is not generic.
        /// </summary>
        public List<RGenericParameter>? GenericParameters { get; set; }

        /// <summary>
        /// Parameters of the function or <see cref="null"/> if the function has no parameters.
        /// </summary>
        public List<RParameter>? Parameters { get; set; }

        public RSignature Signature { get; set; }

        /// <summary>
        /// Return type of the function or <see cref="null"/> if the function has no return type.
        /// </summary>
        public RExpression? ReturnType { get; set; }

        /// <summary>
        /// The body of the function or <see cref="null"/> if the function has no body.
        /// </summary>
        public RStatement? Body { get; set; }

        public RFunction(string name, RSignature signature)
            : this(new RId(name), signature)
        {
        }

        public RFunction(RId id, RSignature signature)
            : base(id)
        {
            Signature = signature;
        }
    }
}
