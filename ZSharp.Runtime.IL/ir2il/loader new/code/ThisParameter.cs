namespace ZSharp.Runtime.NET.IR2IL
{
    internal class ThisParameter(Type declaringType) : IL.ParameterInfo
    {
        public override Type ParameterType => declaringType;

        public override string Name => "this";

        public override int Position => 0;

        public override IL.ParameterAttributes Attributes => IL.ParameterAttributes.None;

        public override object? DefaultValue => null;

        public override bool HasDefaultValue => false;
    }
}
