namespace ZSharp.Compiler
{
    public sealed class AggregateError(params IEnumerable<Error> errors) : Error
    {
        public IEnumerable<Error> Errors { get; private set; } = errors;

        public bool HasErrors => Errors.Any();

        public AggregateError Combine(AggregateError other)
            => Combine(other.Errors);

        public AggregateError Combine(params IEnumerable<Error> moreErrors)
            => new(Errors.Concat(moreErrors));

        public void Append(params IEnumerable<Error> moreErrors)
        {
            Errors = Errors.Concat(moreErrors);
        }

        public void Append(string error)
            => Append(new ErrorMessage(error));

        public override string ToString()
        {
            var errors = string.Join(Environment.NewLine, Errors.Select(e => e.ToString()));
            return $"AggregateError: {Environment.NewLine}{errors}";
        }
    }
}
