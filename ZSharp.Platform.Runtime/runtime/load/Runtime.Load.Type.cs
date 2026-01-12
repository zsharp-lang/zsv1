namespace ZSharp.Platform.Runtime
{
    partial class Runtime
    {
        private Type LoadType(IR.TypeDefinition type)
        {
            if (type.Module is not null)
                throw new InvalidOperationException(
                    $"Cannot directly load type {type} owned by module {type.Module.Name}"
                );

            return Loader.LoadType(type);
        }
    }
}
