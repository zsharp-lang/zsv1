namespace ZSharp.Compiler
{
    /// <summary>
    /// Choosing between the candidates one object stands for. It has nothing to do with
    /// calls in particular: an operation is applied to each candidate, in its own way, and
    /// overloading decides whose result is the result.
    /// </summary>
    public partial class Overloading
    {
        public Overloading Clone() => (Overloading)MemberwiseClone();
    }
}
