using CommonZ.Utils;
using System.Diagnostics.CodeAnalysis;
using ZSharp.IR;

namespace ZSharp.Platform.Runtime
{
    public sealed partial class Runtime
    {
        private readonly List<object> _exposedObjects = [];
        private readonly Queue<ExposedObjectHandle> _avialableHandles = [];

        private Class ir_runtimeType;

        private Global ir_runtimeInstance;

        private Method ir_getExposedObjectFunction;

        public ExposedObjectHandle CreateHandle(object obj)
        {
            if (obj is null)
                throw new ArgumentNullException(nameof(obj), "Cannot create a handle for a null object.");

            ExposedObjectHandle handle = _exposedObjects.Count;

            if (_avialableHandles.Count > 0)
                handle = _avialableHandles.Dequeue();
            else _exposedObjects.Add(null!);

            _exposedObjects[handle] = obj;

            return handle;
        }

        public void DestroyHandle(ExposedObjectHandle handle)
        {
            if (handle < 0 || handle >= _exposedObjects.Count)
                throw new ArgumentOutOfRangeException(nameof(handle), "Invalid exposed object handle.");
            _exposedObjects[handle] = null!;
            _avialableHandles.Enqueue(handle);
        }

        public Collection<IR.VM.Instruction> Expose(object? obj)
        {
            if (obj is null)
                return [
                    new IR.VM.PutNull()
                ];

            var handle = CreateHandle(obj);

            return [
                new IR.VM.GetGlobal(ir_runtimeInstance),
                new IR.VM.PutInt32(handle),
                new IR.VM.Call(ir_getExposedObjectFunction),
            ];
        }

        public object GetExposedObject(ExposedObjectHandle handle)
            => _exposedObjects[handle]!;

        [MemberNotNull(nameof(ir_runtimeType), nameof(ir_getExposedObjectFunction), nameof(ir_runtimeInstance))]
        private void SetupExposeSystem()
        {
            ir_runtimeType = new("Runtime");
            ir_runtimeType.Methods.Add(ir_getExposedObjectFunction = new(TypeSystem.Object));
            ir_getExposedObjectFunction.Signature.Args.Parameters.Add(new("this", new ClassReference(ir_runtimeType)));
            ir_getExposedObjectFunction.Signature.Args.Parameters.Add(new("handle", TypeSystem.SInt32));
            ir_runtimeInstance = new("Runtime.Instance", new ClassReference(ir_runtimeType));

            _typeDefCache[ir_runtimeType] = typeof(Runtime);
            _functionCache.Add(ir_getExposedObjectFunction.UnderlyingFunction, ((Delegate)GetExposedObject).Method);
            _globalCache[ir_runtimeInstance] = typeof(Runtime).GetField(nameof(_instance)) ?? throw new("Internal error");
        }
    }
}
