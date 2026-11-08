using CommonZ;
using ZSharp.Compiler;
using ZSharp.Importer.ILLoader;

using CompilerObject = ZSharp.MIR.CompilerObject;
using Result = CommonZ.Result<ZSharp.MIR.CompilerObject>;
using IResult = IResult<ZSharp.MIR.CompilerObject, CommonZ.Error>;
using Toolchain = Core.Compiler.ModuleScope;

namespace ZSharp.Lang.DotNet
{
    /// <summary>
    /// Runs documents on the CLR. Each document has a frame: an array of objects with one
    /// element for each of its locals. Each statement runs as a function that takes the
    /// frame, so the same code can run with another frame, and no types are made for it.
    /// </summary>
    public sealed class DotNetDocuments(
        Platform.Runtime.Runtime runtime,
        ILLoader il,
        Func<HIR.Instance, Error?> load
    )
        : IDocumentHost
    {
        private readonly Dictionary<Package.ZSharp.Document, Frame> frames = [];
        private readonly Dictionary<LIR.Type, (LIR.FunctionHandle Get, LIR.FunctionHandle Set)> elements = [];

        private readonly LIR.TypeHandle objects = Handle(runtime, typeof(object[]));
        private readonly LIR.TypeHandle index = Handle(runtime, typeof(int));

        /// <summary>
        /// Loads a local of a document as an element of its document's frame.
        /// </summary>
        public void Install(MappedLoader loader)
        {
            loader.Definitions.Register<Package.ZSharp.DocumentLocal>(local =>
            {
                CompilerObject? init = null;

                if (local.Init is not null && Toolchain.HIRCodeLoader.LoadExpression(local.Init).When(out init).Error(out var error))
                    return Result.Error(error);

                var slot = local.Document.Locals.IndexOf(local);

                if (local.Type is not HIR.Infer)
                    return Toolchain.HIRLoader.LoadType(local.Type).When(out var declared).Error(out error)
                        ? Result.Error(error)
                        : Result.Ok(new FrameSlot(this, FrameOf(local.Document), slot, declared!, init));

                if (init is null)
                    return Result.Error($"Local {local.Name} has no initial value to infer its type from.");

                return Toolchain.TS.TypeOf(init).When(out var inferred).Error(out _)
                    ? Result.Error($"The type of local {local.Name} can't be inferred from its initial value.")
                    : Result.Ok(new FrameSlot(this, FrameOf(local.Document), slot, inferred!, init));
            });

            loader.Instances.Register<Package.ZSharp.DocumentLocal>(local => Toolchain.HIRLoader.LoadDefinition(local));
        }

        public Error? Execute(Package.ZSharp.Document document, HIR.Code.Statement statement)
        {
            if (Toolchain.HIRCodeLoader.LoadStatement(statement).When(out var loaded).Error(out var error))
                return error;

            if (Toolchain.IR.CompileStatement(loaded!, runtime).When(out var compiled).Error(out error))
                return error;

            var frame = FrameOf(document);

            runtime.Execute(compiled!, (frame.Parameter, (object?)frame.Values(document)));

            return null;
        }

        public IResult<object?, Error> Evaluate(Package.ZSharp.Document document, HIR.Code.Expression expression)
        {
            if (Toolchain.HIRCodeLoader.LoadExpression(expression).When(out var loaded).Error(out var error))
                return Result<object?>.Error(error);

            if (Toolchain.IR.CompileExpression(loaded!, runtime).When(out var compiled).Error(out error))
                return Result<object?>.Error(error);

            var frame = FrameOf(document);

            return Result<object?>.Ok(runtime.Evaluate(compiled!, (frame.Parameter, (object?)frame.Values(document))));
        }

        public HIR.Code.Expression Described(HIR.Code.Expression type)
            => new HIR.Code.Cast { Value = type, Type = new HIR.Code.Get(il.Load(typeof(HIR.Type))) };

        public Error? Load(HIR.Instance definition) => load(definition);

        private Frame FrameOf(Package.ZSharp.Document document)
        {
            if (!frames.TryGetValue(document, out var frame))
                frames[document] = frame = new(new LIR.Parameter(objects) { Name = "frame" });

            return frame;
        }

        /// <summary>
        /// The functions that read and write an element of a frame as a value of a type.
        /// </summary>
        internal (LIR.FunctionHandle Get, LIR.FunctionHandle Set) Elements(LIR.Type type)
        {
            if (elements.TryGetValue(type, out var known))
                return known;

            var get = new LIR.Signature { ReturnType = type };
            get.Parameters.Add(new(objects) { Name = "frame" });
            get.Parameters.Add(new(index) { Name = "index" });

            var set = new LIR.Signature();
            set.Parameters.Add(new(objects) { Name = "frame" });
            set.Parameters.Add(new(index) { Name = "index" });
            set.Parameters.Add(new(type) { Name = "value" });

            var handles = (Get: new LIR.FunctionHandle(get), Set: new LIR.FunctionHandle(set));

            runtime.MapGetElement(handles.Get, type);
            runtime.MapSetElement(handles.Set, type);

            return elements[type] = handles;
        }

        internal LIR.Constant Index(int slot) => new(index, slot);

        private static LIR.TypeHandle Handle(Platform.Runtime.Runtime runtime, Type type)
        {
            var handle = new LIR.TypeHandle();

            runtime.Map(handle, type);

            return handle;
        }

        /// <summary>
        /// The frame of a document, and the parameter its statements take it through.
        /// </summary>
        internal sealed class Frame(LIR.Parameter parameter)
        {
            private object?[] values = [];

            public LIR.Parameter Parameter { get; } = parameter;

            /// <summary>
            /// The values of the frame, with room for every local the document has.
            /// </summary>
            public object?[] Values(Package.ZSharp.Document document)
            {
                if (values.Length < document.Locals.Count)
                    Array.Resize(ref values, document.Locals.Count);

                return values;
            }
        }
    }

    /// <summary>
    /// A local of a document: an element of the document's frame. Declaring it is where
    /// it takes its initial value.
    /// </summary>
    internal sealed class FrameSlot(
        DotNetDocuments documents,
        DotNetDocuments.Frame frame,
        int slot,
        CompilerObject type,
        CompilerObject? init
    )
        : CompilerObject
        , ICompileIRExpression
        , ICTSet
        , ICTDeclare
        , ITyped
    {
        CompilerObject ITyped.Type => type;

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => Toolchain.IR.CompileType(type, target).When(out var compiled).Error(out var error)
                ? Result<LIR.Expression>.Error(error)
                : Result<LIR.Expression>.Ok(new LIR.Call(
                    documents.Elements(compiled!).Get,
                    new LIR.GetLocal(frame.Parameter),
                    documents.Index(slot)
                ));

        /// <summary>
        /// Code that writes a value to the slot.
        /// </summary>
        public IResult<LIR.Statement, Error> Store(CompilerObject value, object? target)
        {
            if (Toolchain.IR.CompileType(type, target).When(out var compiled).Error(out var error))
                return Result<LIR.Statement>.Error(error);

            if (Toolchain.IR.CompileExpression(value, target).When(out var result).Error(out error))
                return Result<LIR.Statement>.Error(error);

            return Result<LIR.Statement>.Ok(new LIR.Evaluate(new LIR.Call(
                documents.Elements(compiled!).Set,
                new LIR.GetLocal(frame.Parameter),
                documents.Index(slot),
                result!
            )));
        }

        public IResult Set(CompilerObject value)
            => Toolchain.CG.ImplicitCast(value, type).Ok(out var converted)
                ? Result.Ok(new SetSlot(this, converted))
                : Result.Error("The value doesn't match the type of this local.");

        public IResult Declare()
        {
            if (init is null)
                return Result.Ok(new DeclareSlot(this, null, type));

            return Toolchain.CG.ImplicitCast(init, type).Ok(out var converted)
                ? Result.Ok(new DeclareSlot(this, converted, type))
                : Result.Error("The initial value doesn't match the type of this local.");
        }
    }

    /// <summary>
    /// Where a local of a document starts. Without an initial value, it starts with the
    /// default value of its type.
    /// </summary>
    internal sealed class DeclareSlot(FrameSlot slot, CompilerObject? value, CompilerObject type)
        : CompilerObject
        , ICompileIRStatement
    {
        public IResult<LIR.Statement, Error> CompileIRStatement(object? target)
        {
            if (value is not null)
                return slot.Store(value, target);

            return Toolchain.IR.CompileType(type, target).When(out var compiled).Error(out var error)
                ? Result<LIR.Statement>.Error(error)
                : slot.Store(new DefaultValue(compiled!, type), target);
        }
    }

    internal sealed class SetSlot(FrameSlot slot, CompilerObject value)
        : CompilerObject
        , ICompileIRExpression
        , ITyped
    {
        CompilerObject ITyped.Type => ((ITyped)slot).Type;

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
        {
            if (slot.Store(value, target).When(out var store).Error(out var error))
                return Result<LIR.Expression>.Error(error);

            return slot.CompileIRExpression(target).When(out var read).Error(out error)
                ? Result<LIR.Expression>.Error(error)
                : Result<LIR.Expression>.Ok(new LIR.Sequence(new LIR.Block(store!), read!));
        }
    }

    internal sealed class DefaultValue(LIR.Type compiled, CompilerObject type)
        : CompilerObject
        , ICompileIRExpression
        , ITyped
    {
        CompilerObject ITyped.Type => type;

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => Result<LIR.Expression>.Ok(new LIR.Constant(compiled, null));
    }
}
