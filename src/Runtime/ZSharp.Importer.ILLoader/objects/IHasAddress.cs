namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A variable whose address isn't what reading it compiles to, as a field read through
    /// its accessor, or a parameter that already holds a reference.
    /// </summary>
    internal interface IHasAddress
    {
        /// <returns>The code that gives the variable's address, or <see langword="null"/> if it has none.</returns>
        public IResult<LIR.Expression, Error>? AddressOf(object? target);
    }
}
