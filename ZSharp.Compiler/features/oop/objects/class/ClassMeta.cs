namespace ZSharp.Objects
{
    /// <summary>
    /// This class's instance defines the metaclass which all regular classes are
    /// instances of.
    /// </summary>
    public sealed class ClassMeta
        : CompilerObject
        , IMetaClass<Class>
    {
        public Class DefaultBaseClass { get; set; } = new()
        {
            Name = "Object"
        };

        public Class Construct(Compiler.Compiler compiler, string name, CompilerObject[] bases, CompilerObject[]? content)
        {
            var result = new Class()
            {
                Name = name
            };

            bases = bases.Select(compiler.Evaluate).ToArray();

            int baseIndex = 0;

            if (bases.Length > 1 && bases[0] is Class @base)
            {
                result.Base = @base;
                baseIndex = 1;
            }
            else result.Base = DefaultBaseClass;

            for (int i = baseIndex; i < bases.Length; i++)
            {
                throw new NotImplementedException();
                //if (bases[i] is Interface @interface)
                //    throw new NotImplementedException();
                //else if (compiler.TypeSystem.IsTypeModifier<Inline>(bases[i], out Class? inlineBase))
                //    throw new NotImplementedException();
            }

            // TODO: prepare the class content

            if (FindSubclassInitializer(result.Base) is CompilerObject subclassInitializer)
                compiler.Evaluate(compiler.Call(subclassInitializer, [new(result)]));

            return result;
        }

        private CompilerObject? FindSubclassInitializer(CompilerObject? @base)
        {
            //while (@base is not null)
            //    if (@base.SubclassInitializer is not null) return @base.SubclassInitializer;
            //    else @base = @base.Base;

            return null;
        }
    }
}
