using System.Xml.Linq;

namespace ZSharp.Objects
{
    public sealed class ClassMetaClass
        : CompilerObject
    {
        public GenericClass CreateClass(Compiler.Compiler compiler)
        {
            return new();
        }

        public void ConstructClass(
            Compiler.Compiler compiler, 
            GenericClass @class,
            ClassSpec spec
        )
        {
            @class.Name = spec.Name;
            @class.Content.AddRange(spec.Content);

            var bases = spec.Bases;

            if (bases.Length > 0)
            {
                int interfacesIndex = 0;
                if (bases[0] is GenericClassInstance)
                    @class.Base = (GenericClassInstance)bases[interfacesIndex++];

                //for (; interfacesIndex < bases.Length; interfacesIndex++)
                //    if (bases[interfacesIndex] is not IAbstraction @abstract)
                //        throw new();
                //    else Implementations[bases[interfacesIndex]] = new(@abstract, this);
            }

            
        }
    }
}
