namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        /// <summary>
        /// Calls the given <see cref="IR.Function"/> with the given arguments.
        /// If the function is owned by a module, the module must be already loaded.
        /// If the function is not owned by a module, objects referenced by the function
        /// must be already loaded, otherwise an exception will be thrown during the call.
        /// 
        /// The function is not cached, so it will be loaded every time it is called.
        /// </summary>
        /// <param name="function">The function to call.</param>
        /// <param name="arguments">The parameters to call the function with.</param>
        /// <returns><see cref="null"/> if the function returns void. Otherwise, it
        /// returns the return value of the function. In case the function returns null
        /// and it is not marked as void, an <see cref="InvalidFunctionException"/> is thrown.</returns>
        /// <exception cref="InvalidFunctionException>">Thrown when the function returns a value
        /// incompatible with its declared return type (null on a non-void and not null on void)</exception>
        public ZSObject? Call(IR.Function function, params ZSObject[] arguments)
        {
            throw new NotImplementedException();
        }
    }
}
