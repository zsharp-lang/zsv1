using ZSharp.IR.VM;

namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        /// <summary>
        /// Creates a new <see cref="ZSFunction"/> object that represents the given
        /// <paramref name="function"/>, bound to the given <paramref name="self"/> object.
        /// 
        /// The function must be already loaded into the runtime.
        /// Otherwise, a <see cref="FunctionNotLoadedException"/> will be thrown.
        /// 
        /// If the function is owned by a module, the module must be already loaded.
        /// Otherwise, a <see cref="ModuleNotLoadedException"/> will be thrown.
        /// </summary>
        /// <param name="function">The IR of the function to bind.</param>
        /// <returns>A new <see cref="ZSFunction"/> object holding the runtime representation of
        /// the given <paramref name="function"/>, bound to the given <paramref name="self"/>
        /// object.</returns>
        /// <exception cref="ModuleNotLoadedException">Thrown when the function is owned by a module
        /// and the module is not loaded into the runtime.</exception>
        /// <exception cref="FunctionNotLoadedException">Thrown if the function is not already loaded into 
        /// the runtime.</exception></exception>
        public ZSFunction Bind(IR.Function function, ZSObject self)
        {
            if (function.Module is not null && !IsLoaded(function.Module))
                throw new ModuleNotLoadedException<IR.Function>(function);

            if (!TryGet(function, out ZSFunction? result))
                throw new FunctionNotLoadedException(function);

            return Bind(result, self);
        }

        /// <summary>
        /// Creates a new <see cref="ZSFunction"/> object from the given
        /// <paramref name="function"/>, bound to the given <paramref name="self"/> object.
        /// </summary>
        /// <param name="function">The runtime object of the function to copy and bind.</param>
        /// <returns>A new <see cref="ZSFunction"/> object which is a copy of
        /// the given <paramref name="function"/>, bound to the given <paramref name="self"/>
        /// object.</returns>
        public ZSFunction Bind(ZSFunction function, ZSObject self)
            => new(function.IR, 1, TypeSystem.Function)
            {
                ArgumentCount = function.ArgumentCount,
                Code = function.Code,
                LocalCount = function.LocalCount,
                Self = self,
                StackSize = function.StackSize
            };

        /// <summary>
        /// Calls the given <see cref="IR.Function"/> with the given arguments.
        /// If the function is owned by a module, the module must be already loaded.
        /// If the function is not owned by a module, objects referenced by the function
        /// must be already loaded, otherwise an exception will be thrown during the call.
        /// 
        /// The function must be already loaded into the runtime.
        /// Otherwise, a <see cref="FunctionNotLoadedException"/> will be thrown.
        /// </summary>
        /// <param name="function">The function to call.</param>
        /// <param name="arguments">The parameters to call the function with.</param>
        /// <returns><see langword="null"/> if the function returns void. Otherwise, it
        /// returns the return value of the function. In case the function returns null
        /// and it is not marked as void, an <see cref="InvalidFunctionException"/> is thrown.</returns>
        /// <exception cref="InvalidFunctionException>">Thrown when the function returns a value
        /// incompatible with its declared return type (null on a non-void and not null on void)</exception>
        /// <exception cref="FunctionNotLoadedException">Thrown if the function is not already loaded into 
        /// the runtime.</exception></exception>
        public ZSObject? Call(IR.Function function, params ZSObject[] arguments)
        {
            var zsFunction = Get(function);

            return Call(zsFunction, arguments);
        }

        public ZSObject? Call(ZSFunction function, params ZSObject[] arguments)
        {
            PushFrame(Frame.NoArguments(0, [], 1));
            Run(new Frame(arguments, function.LocalCount, function.Code, function.StackSize));
            
            return PopFrame().Pop();
        }
    }
}
