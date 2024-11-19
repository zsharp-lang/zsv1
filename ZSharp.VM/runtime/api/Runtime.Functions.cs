namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        /// <summary>
        /// Creates a new <see cref="ZSMethod"/> object that represents the given
        /// <paramref name="method"/>, bound to the given <paramref name="self"/> object.
        /// 
        /// The method must be already loaded into the runtime.
        /// Otherwise, a <see cref="IRObjectNotLoadedException{IR.Method}"/> will be thrown.
        /// 
        /// The method'w owning module must be already loaded.
        /// Otherwise, a <see cref="ModuleNotLoadedException"/> will be thrown.
        /// </summary>
        /// <param name="method">The IR of the method to bind.</param>
        /// <returns>A new <see cref="ZSMethod"/> object holding the runtime representation of
        /// the given <paramref name="method"/>, bound to the given <paramref name="self"/>
        /// object.</returns>
        /// <exception cref="ModuleNotLoadedException">Thrown when the method's owning module is 
        /// not loaded into the runtime.</exception>
        /// <exception cref="IRObjectNotLoadedException{IR.Method}">Thrown if the method is not already loaded into 
        /// the runtime.</exception>
        public ZSMethod Bind(IR.Method method, ZSObject self)
        {
            if (method.Module is not null && !IsLoaded(method.Module))
                throw new ModuleNotLoadedException<IR.Method>(method);

            if (!TryGet(method, out ZSMethod? result))
                throw new IRObjectNotLoadedException<IR.Method>(method);

            return Bind(result, self);
        }

        /// <summary>
        /// Creates a new <see cref="ZSFunction"/> object from the given
        /// <paramref name="method"/>, bound to the given <paramref name="self"/> object.
        /// </summary>
        /// <param name="method">The runtime object of the function to copy and bind.</param>
        /// <returns>A new <see cref="ZSFunction"/> object which is a copy of
        /// the given <paramref name="method"/>, bound to the given <paramref name="self"/>
        /// object.</returns>
        public ZSMethod Bind(ZSMethod method, ZSObject self)
            => ZSMethod.CreateFrom(method.IR, method.Code, method.Type, self);

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

        public ZSObject? Call(IR.Method method, params ZSObject[] arguments)
        {
            var zsMethod = Get(method);

            return Call(zsMethod, arguments);
        }

        public ZSObject? Call(ZSMethod method, params ZSObject[] arguments)
        {
            PushFrame(Frame.NoArguments(0, [], 1));
            Run(new Frame(arguments, method.LocalCount, method.Code, method.StackSize));

            return PopFrame().Pop();
        }
    }
}
