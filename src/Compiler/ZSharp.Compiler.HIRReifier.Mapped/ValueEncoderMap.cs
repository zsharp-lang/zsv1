namespace ZSharp.Compiler
{
    /// <summary>
    /// Maps the type of a value to how it is encoded into code. A value uses the
    /// encoder of its own type, or else of the nearest base class, or else of the first
    /// interface it implements that has one.
    /// </summary>
    public sealed class ValueEncoderMap
    {
        private readonly Dictionary<Type, Func<object, IResult<HIR.Code.Expression, Error>>> encoders = [];

        /// <summary>
        /// Registers an encoder, replacing any existing one for that type.
        /// </summary>
        /// <returns>The replaced encoder, or <see langword="null"/> if there was none.</returns>
        public Func<object, IResult<HIR.Code.Expression, Error>>? Register<T>(Func<T, IResult<HIR.Code.Expression, Error>> encoder)
            where T : notnull
        {
            encoders.TryGetValue(typeof(T), out var previous);

            encoders[typeof(T)] = value => encoder((T)value);

            return previous;
        }

        public IResult<HIR.Code.Expression, Error> Encode(object? value)
        {
            if (value is null)
                return Result<HIR.Code.Expression>.Error(new UnsupportedError("No encoder is registered for null."));

            for (var type = value.GetType(); type is not null; type = type.BaseType)
                if (encoders.TryGetValue(type, out var encoder) && type != typeof(object))
                    return encoder(value);

            foreach (var @interface in value.GetType().GetInterfaces())
                if (encoders.TryGetValue(@interface, out var encoder))
                    return encoder(value);

            return encoders.TryGetValue(typeof(object), out var fallback)
                ? fallback(value)
                : Result<HIR.Code.Expression>.Error(new UnsupportedError($"No encoder is registered for {value.GetType().Name}."));
        }
    }
}
