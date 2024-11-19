using ZSharp.CGObjects;

namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        public CGObject CreateClass(string? name)
            => CreateClass(Context.DefaultMetaClass, name);

        public CGObject CreateClass(CGObject meta, string? name)
            => CreateClass(new ClassSpec
            {
                Name = name,
                MetaClass = meta
            });

        public CGObject CreateClass(ClassSpec spec)
        {
            var cgObject = Call(spec.MetaClass, [ new(spec) ]);

            if (cgObject is RawCode rawCode)
                cgObject = CreateRuntimeObject(Evaluate(rawCode));

            return cgObject;
        }

        public void CompileClassItem(CGObject @class, ClassSpec spec, CGObject item)
        {
            if (spec.MetaClass is ICTMetaClass ctMeta)
                ctMeta.Compile(this, @class, item);

            // TODO: Z#
        }
    }
}
