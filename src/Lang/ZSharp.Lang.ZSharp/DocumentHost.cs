namespace ZSharp.Lang
{
    /// <summary>
    /// A context that is a document.
    /// </summary>
    public interface IDocument
    {
        public Package.ZSharp.Document Document { get; }
    }

    /// <summary>
    /// What running a document needs from what runs it.
    /// </summary>
    public interface IDocumentHost
    {
        /// <summary>
        /// Runs a statement of the document, with the document's locals.
        /// </summary>
        public Error? Execute(Package.ZSharp.Document document, HIRStatement statement);

        /// <summary>
        /// Evaluates an expression of the document, with the document's locals.
        /// </summary>
        public IResult<object?, Error> Evaluate(Package.ZSharp.Document document, HIRExpression expression);

        /// <summary>
        /// Code that gives the value of code written as a type where an HIR type goes, so
        /// it converts as any value going there does: a type to what describes it.
        /// </summary>
        public HIRExpression Described(HIRExpression type);

        /// <summary>
        /// Loads a definition that was just built, returning why it could not be.
        /// </summary>
        public Error? Load(HIR.Instance definition);
    }
}
