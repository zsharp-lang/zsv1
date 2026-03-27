using ZSharp.Compiler.Features;

namespace ZSharp.Importer.ILLoader.Objects
{
    partial class OverloadGroup
    {
        private readonly List<ProxiedObject<IOverload>> overloads = [];

        public bool AddOverload(CompilerObject @object)
        {
            if (!@object.Is<IOverload>(out var overload))
                return false;

            overloads.Add(new()
            {
                Capability = overload,
                Origin = @object
            });
            return true;
        }
    }
}
