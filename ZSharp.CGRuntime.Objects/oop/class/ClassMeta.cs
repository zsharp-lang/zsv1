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
        public void Compile(ICompiler compiler, Class result, ClassSpec spec)
        {
            Class? @baseClass = null;

            if (spec.Bases is not null)
                foreach (var @base in spec.Bases)
                {
                    if (@base is Class @class)
                    {
                        if (baseClass is not null)
                            throw new($"Class may only inherit a single class");
                        baseClass = @class;
                    }

                    // TODO: interfaces
                }

            result.IR = new(result.Name, result.Base?.IR);

            if (spec.Content is not null)
                foreach (var item in spec.Content)
                {
                    switch (item)
                    {
                        case Field field:
                            result.Members.Add(field.Name, field);
                            result.IR.Fields.Add(field.IR!);
                            break;
                        case Method method:
                            result.IR.Methods.Add(method.IR!);
                            if (method.Name is not null)
                            result.Members.Add(method.Name, method); 
                            break;
                        case Class @class
                            when @class.Name is not null:
                            result.Members.Add(@class.Name, @class); break;
                        default:
                            throw new NotImplementedException(item.GetType().Name);
                    }
                }
        }

        public Class Construct(ICompiler compiler, ClassSpec spec)
            => new()
            {
                Name = spec.Name
            };
    }
}
