using ZSharp.IR;

namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        /// <summary>
        /// Gets the value of the given <see cref="FieldAttributes.Static"/> field.
        /// 
        /// The given field must have the <see cref="FieldAttributes.Static"/> flag set.
        /// Passing a non-static field will result in a <see cref="UnboundFieldException"/>.
        /// </summary>
        /// <param name="field">The IR of the field to get the value of.</param>
        /// <returns>The value associated with the given field.</returns>
        /// <exception cref="UnboundFieldException">Thrown when the given field is not static.</exception>"
        public ZSObject GetUnsafe(Field field)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the value of the given field from the given instance.
        /// 
        /// If the field is static, the instance will be ignored and may be <see langword="null"/>.
        /// </summary>
        /// <param name="field">The IR of the field to get the value of.</param>
        /// <param name="instance">The instance of which to get the field from.</param>
        /// <returns>The value of the given <paramref name="field"/> in the given <paramref name="instance"/>.</returns>
        public ZSObject GetUnsafe(Field field, ZSStruct? instance)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the value of the given global variable.
        /// 
        /// The global's owning module must be loaded into the runtime.
        /// Otherwise, a <see cref="ModuleNotLoadedException"/> will be thrown.
        /// </summary>
        /// <param name="global">The IR of the global to get the value of.</param>
        /// <returns>The value of the given <paramref name="global"/>.</returns>
        /// <exception cref="ModuleNotLoadedException">Thrown when the given 
        /// global's owning module is not loaded.</exception>
        public ZSObject GetUnsafe(Global global)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the value of the given global variable from the given module.
        /// </summary>
        /// <param name="global">The IR of the global to get the value of.</param>
        /// <param name="module">The module to get the global from.</param>
        /// <returns>The value of the given <paramref name="global"/>.</returns>
        public ZSObject GetUnsafe(Global global, ZSModule module)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the value of the given local variable.
        /// </summary>
        /// <param name="local">The IR of the local variable to get.</param>
        /// <returns>The value associated with the given <paramref name="local"/></returns>
        public ZSObject GetUnsafe(IR.VM.Local local)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the value of the given parameter.
        /// </summary>
        /// <param name="parameter">The IR of the parameter to get.</param>
        /// <returns>The value associated with the given <paramref name="parameter"/>.</returns>
        public ZSObject GetUnsafe(Parameter parameter)
        {
            throw new NotImplementedException();
        }
    }
}
