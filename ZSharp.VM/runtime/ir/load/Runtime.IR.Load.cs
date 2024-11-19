using ZSharp.IR;

namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        /// <summary>
        /// Use <see cref="ImportIR(Class)"/> instead.
        /// </summary>
        /// <param name="class"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public ZSClass LoadIR(Class @class)
        {
            if (@class.Module is not null)
                throw new Exception("LoadIR(Class) can only be used with free functions");

            return irMap.Cache(@class, LoadIRInternal(@class));
        }

        /// <summary>
        /// Use <see cref="ImportIR(Function)"/> instead.
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public ZSFunction LoadIR(Function function)
        {
            if (function.Module is not null)
                throw new Exception("LoadIR(Function) can only be used with free functions");

            return irMap.Cache(function, LoadIRInternal(function));
        }

        public ZSMethod LoadIR(Method method)
        {
            if (method.Module is not null)
                throw new Exception("LoadIR(Function) can only be used with free functions");

            return irMap.Cache(method, LoadIRInternal(method));
        }

        /// <summary>
        /// Use <see cref="ImportIR(Module)"/> instead.
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public ZSModule LoadIR(Module module)
            => LoadIRInternal(module);
    }
}
