namespace ZSharp.VM
{
    public sealed class TypeSystem
    {
        private readonly Interpreter _interpreter;

        internal static Types.Type TypeInternal { get; } = new Types.Type();

        public static ZSObject Any { get; } = new Types.Any();

        //public static ZSObject Module { get; } = new Types.Module();

        public static ZSObject Null { get; } = new Types.Null();

        public static ZSObject String { get; } = new Types.String();

        public static ZSObject Type => TypeInternal;

        public static ZSObject Void { get; } = new Types.Void();

        internal TypeSystem(Interpreter interpreter)
        {
            _interpreter = interpreter;
        }

        public bool IsAssignableTo(ZSObject source, ZSObject target)
        {
            bool? sourceToTarget;
            if (source is IType sourceType)
                sourceToTarget = sourceType.IsAssignableTo(_interpreter, target);
            // TODO: implement
            //else if (_interpreter.IsInstance(source, Type))
            //    sourceToTarget = _interpreter.Call(TypeInternal.Methods.IsAssignableTo, source, target);
            else
                throw new ArgumentException("Must be a valid type", nameof(source));

            if (sourceToTarget is false) return false;

            bool? targetFromSource;
            if (target is IType targetType)
                targetFromSource = targetType.IsAssignableFrom(_interpreter, source);
            // TODO: implement
            //else if (_interpreter.IsInstance(target, Type))
            //    targetFromSource = _interpreter.Call(TypeInternal.Methods.IsAssignableFrom, target, source);
            else
                throw new ArgumentException("Must be a valid type", nameof(target));

            if (targetFromSource is false) return false;

            return sourceToTarget ?? targetFromSource ?? false;
        }

        public bool IsAssignableFrom(ZSObject target, ZSObject source)
        {
            return IsAssignableTo(source, target);
        }
    }
}
