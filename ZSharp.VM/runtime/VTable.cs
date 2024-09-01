using CommonZ.Utils;

namespace ZSharp.VM
{
    internal sealed class VTable
    {
        private readonly Mapping<ZSFunction, ZSFunction> mapping = [];

        /// <summary>
        /// Get the override of the given function.
        /// </summary>
        /// <param name="virtual">The virtual function which is overriden.</param>
        /// <returns>The function that overrides the given vitual function.</returns>
        public ZSFunction this[ZSFunction @virtual]
        {
            get => mapping[@virtual];
            set => mapping[@virtual] = value;
        }
    }
}
