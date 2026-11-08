namespace ZSharp.Platform.Runtime
{
    partial class Runtime
    {
        private readonly Dictionary<LIR.Function, Standalone> standalones = [];

        private readonly Queue<Action> standaloneBodies = [];

        /// <summary>
        /// A function that belongs to no module is loaded as a method of its own, a
        /// <c>DynamicMethod</c>. Its body is emitted once what is being loaded is complete,
        /// since a <c>DynamicMethod</c> can only call a method of a type that exists. A call
        /// to itself, or to another such function that calls it back, finds it declared.
        /// </summary>
        private IL.MethodInfo DeclareStandalone(LIR.Function function)
        {
            var method = new Emit.DynamicMethod(
                function.Name ?? string.Empty,
                ImportType(function.Signature.ReturnType),
                [.. function.Signature.Parameters.Select(parameter => ImportType(parameter.Type))],
                module,
                skipVisibility: true
            );

            functions[function] = method;

            standaloneBodies.Enqueue(() => new CodeEmitter(this, method.GetILGenerator(), function, standalone: true).EmitBody());

            return method;
        }

        /// <summary>
        /// Emits the bodies of the standalone functions declared while loading, and loads
        /// and completes whatever those reach in turn.
        /// </summary>
        private void Finish()
        {
            while (standaloneBodies.TryDequeue(out var standalone))
            {
                standalone();

                while (bodies.TryDequeue(out var body))
                    body();

                Complete();
            }
        }

        /// <summary>
        /// Code of a type the runtime builds can't call a <c>DynamicMethod</c>, which has no
        /// metadata token, so it calls one through a delegate of it, made the first time it
        /// is called. Its type is the delegate type of the function's signature.
        /// </summary>
        internal (int Index, Type Type) StandaloneDelegate(LIR.Function function)
        {
            if (!standalones.TryGetValue(function, out var standalone))
            {
                var method = (Emit.DynamicMethod)ImportFunction(function);
                var type = System.Linq.Expressions.Expression.GetDelegateType([
                    .. method.GetParameters().Select(parameter => parameter.ParameterType),
                    method.ReturnType
                ]);

                standalones[function] = standalone = new(type, () => method.CreateDelegate(type));
                standalone.Index = ExposedObjects.Add(standalone);
            }

            return (standalone.Index, standalone.Type);
        }
    }

    /// <summary>
    /// The delegate code of a built type calls a standalone function through, made the
    /// first time it is needed, when the function is complete.
    /// </summary>
    public sealed class Standalone(Type type, Func<Delegate> make)
    {
        private Delegate? value;

        internal Type Type { get; } = type;

        internal int Index { get; set; }

        public Delegate Delegate => value ??= make();
    }
}
