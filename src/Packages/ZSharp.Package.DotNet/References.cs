using ZSharp.Importer.ILLoader;

namespace Package.DotNet
{
    /// <summary>
    /// The types of references to variables, which <c>ref</c> and <c>out</c> parameters
    /// take. They are HIR's alone, as what a reference is to is a type the CLR has.
    /// </summary>
    [ModuleScope]
    public static class References
    {
        /// <summary>
        /// The type of a reference to a variable, as <c>Ref[Int32]</c>, which a <c>ref</c>
        /// parameter takes.
        /// </summary>
        [Alias("Ref")]
        public static readonly ZSharp.HIR.Instance Ref = new RefFamily(isOut: false);

        /// <summary>
        /// The type of a reference to a variable the callee only assigns, as
        /// <c>Out[Int32]</c>, which an <c>out</c> parameter takes.
        /// </summary>
        [Alias("Out")]
        public static readonly ZSharp.HIR.Instance Out = new RefFamily(isOut: true);
    }
}
