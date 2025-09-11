using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class CommonBaseType
        : IInferredTypeResolver
    {
        IType IInferredTypeResolver.Resolve(Compiler.Compiler compiler, InferredType inferredType)
        {
            List<(IClass, int)> classes = [];

            IType? commonBase = null;
            int minDepth = int.MaxValue;

            foreach (var type in inferredType.CollectedTypes)
            {
                if (type is not IClass @class)
                {
                    if (commonBase is not null && !compiler.TypeSystem.AreEqual(commonBase, type))
                        throw new();
                    commonBase = type;

                    continue;
                }

                int depth = 0;

                for (; @class.Base is not null; @class = @class.Base)
                    depth++;

                if (depth < minDepth)
                    (commonBase, minDepth) = (@class, depth);

                classes.Add((@class, depth));
            }

            if (commonBase is null)
                throw new InvalidOperationException();

            foreach (var (@class, depth) in classes)
            {
                var @base = @class;
                while (depth > minDepth) @base = @base!.Base;

                if (!compiler.TypeSystem.AreEqual(@base!, commonBase))
                    throw new InvalidOperationException();
            }

            return commonBase;
        }
    }
}
