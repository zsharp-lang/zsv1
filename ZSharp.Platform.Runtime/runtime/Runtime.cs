namespace ZSharp.Platform.Runtime
{
    public sealed partial class Runtime
    {
        public Runtime(TypeSystem typeSystem)
        {
            if (_instance is not null)
                throw new InvalidOperationException("Runtime instance already exists.");
            _instance = this;

            TypeSystem = typeSystem;

            SetupExposeSystem();

            Loader = new(this);

            foreach (var (ir, il) in (IEnumerable<(IR.OOPTypeReference, Type)>)[
                (TypeSystem.Void, typeof(void)),
                (TypeSystem.Boolean, typeof(bool)),
                (TypeSystem.Object, typeof(object)),
                (TypeSystem.String, typeof(string)),
                (TypeSystem.SInt32, typeof(int)),
            ])
                _typeDefCache.Add(ir.Definition, il);
        }
    }
}
