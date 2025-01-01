using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public interface IOverload
    {
        /// <summary>
        /// Match the given arguments to this overload.
        /// If the arguments don't match, return null.
        /// </summary>
        /// <param name="arguments">The arguments to match.</param>
        /// <returns>A <see cref="OverloadMatchResult"/> describing how to pass the arguments
        /// to the overload if the arguments match the overload, otherwise <see cref="null"/></returns>
        //public OverloadMatchResult? Match(Compiler.Compiler compiler, Argument[] arguments);
    }
}
