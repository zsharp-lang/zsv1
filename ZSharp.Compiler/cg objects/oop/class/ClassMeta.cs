using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    /// <summary>
    /// This class's instance defines the metaclass which all regular classes are
    /// instances of.
    /// </summary>
    public sealed class ClassMeta
        : CGObject
        , ICTMetaClass<Class>
    {
        public void Compile(Compiler.Compiler compiler, Class result, CGObject item)
        {
            //Class? @baseClass = null;

            //if (spec.Bases is not null)
            //    foreach (var @base in spec.Bases)
            //    {
            //        // TODO: allow bases to customize how they are inherited

            //        if (@base is Class @class)
            //        {
            //            if (baseClass is not null)
            //                throw new($"Class may only inherit a single class");
            //            baseClass = @class;
            //        }

            //        // TODO: interfaces
            //    }

            //result.IR!.Base = result.Base?.IR;

            switch (item)
            {
                case Field field:
                    result.Members.Add(field.Name, field);
                    result.IR.Fields.Add(field.IR!);
                    break;
                case Method method:
                    if (method.Name is not null)
                        result.Members.Add(method.Name, method);
                    result.IR.Methods.Add(method.IR!);
                    break;
                case Class @class:
                    if (@class.Name is not null)
                        result.Members.Add(@class.Name, @class); 
                    //result.IR.NestedTypes.Add(@class.IR!);
                    break;
                default:
                    throw new NotImplementedException(item.GetType().Name);
            }
        }

        public Class Construct(Compiler.Compiler compiler, ClassSpec spec)
            => new()
            {
                Name = spec.Name,
                IR = new(spec.Name),
            };
    }
}
