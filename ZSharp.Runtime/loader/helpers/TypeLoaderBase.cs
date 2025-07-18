namespace ZSharp.Runtime.Loaders
{
    internal abstract class TypeLoaderBase<T>(EmitLoader loader)
        : LoaderBase(loader)
        where T: IR.OOPType
    {
        public required Emit.TypeBuilder ILType { get; init; }

        public required T IRType { get; init; }

        public required TaskManager Tasks { get; init; }

        public Type Load()
        {
            Loader.Runtime.AddTypeDefinition(IRType, ILType);

            DoLoad();

            AddTask(() => AddTask(() => Loader.Runtime.SetTypeDefinition(IRType, ILType.CreateType())));

            return ILType;
        }

        protected abstract void DoLoad();

        protected virtual void AddTask(Action task)
            => Tasks.AddTask(task);
    }
}
