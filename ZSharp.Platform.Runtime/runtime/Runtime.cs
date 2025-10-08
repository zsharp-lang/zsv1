namespace ZSharp.Platform.Runtime
{
    public sealed partial class Runtime
    {
        public bool DebugEnabled { get; }

        public Runtime(TypeSystem typeSystem, bool debugging = true)
        {
            if (_instance is not null)
                throw new InvalidOperationException("Runtime instance already exists.");
            _instance = this;

            EvaluationContextFactory =
                (DebugEnabled = debugging)
                ? new DebuggableEvaluationContextFactory()
                {
                    OutputPath = Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "generated")).FullName
                }
                : new ExecutionEvaluationContextFactory()
                ;

            TypeSystem = typeSystem;

            SetupExposeSystem();

            Loader = new(this);

            foreach (var (ir, il) in (IEnumerable<(IR.TypeReference, Type)>)[
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
