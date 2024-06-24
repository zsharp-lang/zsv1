using CommonZ.Utils;

namespace ZSharp.IR
{
    public class GenericParameter
    {
        public string Name { get; set; }

        public GenericParameterAttributes Attributes { get; set; } = GenericParameterAttributes.None;

        //public Collection<Constraint> Constraints { get; set; }
    }
}
