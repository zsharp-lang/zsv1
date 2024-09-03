using ZSharp.IR;

namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        /// <summary>
        /// Loads a <typeparamref name="T"/> object into the runtime.
        /// If the given <paramref name="ir"/> has already been loaded, the existing
        /// object will be returned.
        /// 
        /// If the <paramref name="ir"/> belongs to a module and the <paramref name="loadModule"/> parameter
        /// is set to <see langword="true"/>, the module will be loaded into the runtime as well.
        /// If <paramref name="loadModule"/> is set to <see langword="false"/>, the module
        /// must be already loaded into the module, otherwise a <see cref="ModuleNotLoadedException"/>
        /// will be thrown.
        /// </summary>
        /// <param name="ir">The IR object to load.</param>
        /// <returns>A <see cref="ZSIRObject{T}"/> instance that holds the state of the ir object.</returns>
        /// <exception cref="ModuleNotLoadedException">Thrown when <paramref name="loadModule"/> is set
        /// to <see langword="false"/> and the class's owning module is not loaded into the runtime.</exception>
        public ZSIRObject<T> ImportIR<T>(T ir)
            where T : IRObject
            => (ir switch
            {
                Class @class => ImportIR(@class),
                Function function => ImportIR(function),
                Module module => ImportIR(module),
                _ => (null as ZSObject) ?? throw new NotImplementedException()
            } as ZSIRObject<T>) 
            ?? throw new($"{ir.GetType().Name} doesn't have a {nameof(ZSIRObject<T>)}<{typeof(T).Name}> implementation");

        /// <summary>
        /// Loads a <see cref="Class"/> object into the runtime.
        /// If the given <paramref name="@class"/> has already been loaded, the existing
        /// class object will be returned.
        /// 
        /// If the class belongs to a module and the <paramref name="loadModule"/> parameter
        /// is set to <see langword="true"/>, the module will be loaded into the runtime as well.
        /// If <paramref name="loadModule"/> is set to <see langword="false"/>, the module
        /// must be already loaded into the module, otherwise a <see cref="ModuleNotLoadedException"/>
        /// will be thrown.
        /// </summary>
        /// <param name="class">The IR of the class to load.</param>
        /// <returns>A <see cref="ZSClass"/> instance that holds the state of the class.</returns>
        /// <exception cref="ModuleNotLoadedException">Thrown when <paramref name="loadModule"/> is set
        /// to <see langword="false"/> and the class's owning module is not loaded into the runtime.</exception>
        public ZSClass ImportIR(Class @class)
        {
            if (TryGet(@class, out var result))
                return result;

            return LoadIR(@class);
        }

        /// <summary>
        /// Loads a <see cref="Function"/> object into the runtime.
        /// If the given <paramref name="function"/> has already been loaded, the existing
        /// function object will be returned.
        /// 
        /// The module must be already loaded into the module, otherwise a 
        /// <see cref="ModuleNotLoadedException"/> will be thrown.
        /// </summary>
        /// <param name="function">The IR of the function to load.</param>
        /// <returns>A <see cref="ZSFunction"/> instance that holds the function runtime information.</returns>
        /// <exception cref="ModuleNotLoadedException">Thrown when <paramref name="loadModule"/> is set
        /// to <see langword="false"/> and the function's owning module is not loaded into the runtime.</exception>
        public ZSFunction ImportIR(Function function)
        {
            if (TryGet(function, out var result))
                return result;

            return LoadIR(function);
        }

        /// <summary>
        /// Imports a <see cref="Module"/> object into the runtime.
        /// If the given <paramref name="module"/> has already been imported, the existing
        /// module object will be returned.
        /// </summary>
        /// <param name="module">The IR of the module to import.</param>
        /// <returns>A <see cref="ZSModule"/> instance that holds the state of the module.</returns>
        public ZSModule ImportIR(Module module)
        {
            if (TryGet(module, out var result))
                return result;

            return LoadIR(module);
        }
    }
}
