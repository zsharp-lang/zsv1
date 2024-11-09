using CommonZ.Utils;

namespace ZSharp.VM
{
    internal sealed class VTable
    {
        private readonly Mapping<ZSMethod, ZSMethod> mapping = [];

        /// <summary>
        /// Get the override of the given function.
        /// </summary>
        /// <param name="virtual">The virtual function which is overriden.</param>
        /// <returns>The function that overrides the given vitual function.</returns>
        public ZSMethod this[ZSMethod @virtual]
        {
            get => mapping[@virtual];
            set => mapping[@virtual] = value;
        }
    }
}
