using System.Diagnostics.CodeAnalysis;
using ZSharp.IR;

namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        ///// <summary>
        ///// Returns the object that was previously bound to the given IR object.
        ///// 
        ///// The IR must be already loaded into the runtime, otherwise a <see cref="IRObjectNotLoadedException{T}"/>
        ///// is thrown.
        ///// </summary>
        ///// <param name="ir">The IR of which the requested object is bound to.</param>
        ///// <returns>The object which was bound to the given <paramref name="ir"/> when
        ///// loading the IR into the runtime.</returns>
        ///// <exception cref="IRObjectNotLoadedException{T}">Thrown when the given IR object is not 
        ///// loaded into the runtime.</exception>
        //public ZSIRObject<T> Get<T>(T ir)
        //    where T : IRObject
        //    => (ir switch
        //    {
        //        Class @class => Get(@class),
        //        Function function => Get(function),
        //        Module module => Get(module),
        //        _ => (null as ZSObject) ?? throw new NotImplementedException()
        //    } as ZSIRObject<T>)
        //    ?? throw new($"{ir.GetType().Name} doesn't have a {nameof(ZSIRObject<T>)}<{typeof(T).Name}> implementation");

        /// <summary>
        /// Returns the object holding the state of the given class.
        /// 
        /// The class must be loaded into the runtime.
        /// Otherwise, a <see cref="IRObjectNotLoadedException{Class}"/> will be thrown.
        /// 
        /// The class's owning module must be loaded into the runtime.
        /// Otherwise, a <see cref="ModuleNotLoadedException"/> will be thrown.
        /// </summary>
        /// <param name="class">The IR of the class to return the state object of.</param>
        /// <returns>A <see cref="ZSModule"/> object holding the state of the given class.</returns>
        /// <exception cref="ModuleNotLoadedException">Thrown when the given class's owning module is 
        /// not loaded.</exception>
        /// <exception cref="IRObjectNotLoadedException{Class}">Thrown when the given class is not 
        /// loaded.</exception>
        public ZSClass Get(Class @class)
        {
            if (@class.Module is not null && !IsLoaded(@class.Module))
                throw new ModuleNotLoadedException<Class>(@class);

            if (!TryGet(@class, out ZSClass? zsClass))
                throw new IRObjectNotLoadedException<Class>(@class);

            return zsClass;
        }

        /// <summary>
        /// Returns the value of the given field.
        /// 
        /// The given field must have the <see cref="FieldAttributes.Static"/> flag set.
        /// Passing a non-static field will result in a <see cref="UnboundFieldException"/>.
        /// 
        /// The field's owning module must be loaded into the runtime.
        /// Otherwise, a <see cref="ModuleNotLoadedException"/> will be thrown.
        /// </summary>
        /// <param name="field">The IR of the field to get the value of.</param>
        /// <returns>The value associated with the given <paramref name="field"/>.</returns>
        /// <exception cref="UnboundFieldException">Thrown when the given field is not static.</exception>
        /// <exception cref="ModuleNotLoadedException">Thrown when the given field's owning module
        /// is not loaded.</exception>
        public ZSObject Get(Field field)
            => Get(field, null);

        /// <summary>
        /// Returns the value of the given field from the given instance.
        /// 
        /// If the field is static, the instance will be ignored and may be <see langword="null"/>.
        /// Otherwise, the given instance must be a valid instance of the field's type.
        /// 
        /// The field's owning module must be loaded into the runtime.
        /// Otherwise, a <see cref="ModuleNotLoadedException"/> will be thrown.
        /// </summary>
        /// <param name="field">The IR of the field to get the value of.</param>
        /// <param name="object">The instance of which to get the field from.</param>
        /// <returns>The value of the given <paramref name="field"/> in the given <paramref name="object"/>.</returns>
        /// <exception cref="TypeError">Thrown when the given <paramref name="object"/> 
        /// is not a valid instance of the field's type.</exception>
        /// <exception cref="ModuleNotLoadedException">Thrown when the given field's owning module
        /// is not loaded.</exception>
        public ZSObject Get(Field field, ZSStruct? @object)
        {
            if (field.Owner is null)
                throw new("Invalid field.");

            if (field.Module is not null && !IsLoaded(field.Module))
                throw new ModuleNotLoadedException<Field>(field);

            if (field.IsStatic)
                return Get(field.Owner).GetField(field);

            if (@object is null) 
                throw new ArgumentNullException(
                    nameof(@object), 
                    "Object must not be null when the field is not static."
                );

            if (field.IsClass)
            {
                if (@object is not ZSClass @class)
                    throw new TypeError("Object must be a class.");

                // TODO: Check if the instance is a subclass of the field's owner
                //if (!@class.IsSubclassOf(field.Owner))
                //    throw new TypeError("Instance is not a subclass of the field's owner!");

                return @class.GetField(field);
            }

            if (@object is not ZSInstance instance)
                throw new TypeError("Object must be an instance.");

            // TODO: Check if the instance is a valid instance of the field's owner
            //if (!IsInstance(instance, field.Owner))
            //    throw new TypeError("Instance is not a valid instance of the field's type!");

            return instance.GetField(field);
        }

        /// <summary>
        /// Returns the object holding runtime representation of the given function.
        /// 
        /// The function must be loaded into the runtime.
        /// Otherwise, a <see cref="IRObjectNotLoadedException{Function}"/> will be thrown.
        /// 
        /// The function's owning module must be loaded into the runtime.
        /// Otherwise, a <see cref="ModuleNotLoadedException"/> will be thrown.
        /// </summary>
        /// <param name="function">The IR of the function to return the runtime representation of.</param>
        /// <returns>A <see cref="ZSFunction"/> object holding the runtime representation of the given function.</returns>
        public ZSFunction Get(Function function)
        {
            if (function.Module is not null && !IsLoaded(function.Module))
                throw new ModuleNotLoadedException<Function>(function);

            if (!TryGet(function, out ZSFunction? zsFunction))
                throw new IRObjectNotLoadedException<Function>(function);

            return zsFunction;
        }

        /// <summary>
        /// Returns the object holding runtime representation of the given method.
        /// 
        /// The method must be loaded into the runtime.
        /// Otherwise, a <see cref="IRObjectNotLoadedException{Method}"/> will be thrown.
        /// 
        /// The method's owning module must be loaded into the runtime.
        /// Otherwise, a <see cref="ModuleNotLoadedException"/> will be thrown.
        /// </summary>
        /// <param name="method">The IR of the method to return the runtime representation of.</param>
        /// <returns>A <see cref="ZSMethod"/> object holding the runtime representation of the given method.</returns>
        public ZSMethod Get(Method method)
        {
            if (method.Module is not null && !IsLoaded(method.Module))
                throw new ModuleNotLoadedException<Method>(method);

            if (!TryGet(method, out ZSMethod? zsMethod))
                throw new IRObjectNotLoadedException<Method>(method);

            return zsMethod;
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
        public ZSObject Get(Global global)
        {
            if (global.Module is null)
                throw new("Invalid global.");

            if (!IsLoaded(global.Module))
                throw new ModuleNotLoadedException<Global>(global);

            return Get(global.Module).GetGlobal(global);
        }

        /// <summary>
        /// Returns the value of the given local variable.
        /// 
        /// The local's owning function must be the currently executing function.
        /// Otherwise, a <see cref="UnboundLocalException"/> will be thrown.
        /// 
        /// The local's owning module - if it has one - must be loaded into the runtime.
        /// Otherwise, a <see cref="ModuleNotLoadedException"/> will be thrown.
        /// </summary>
        /// <param name="local">The IR of the local variable to get.</param>
        /// <returns>The value associated with the given <paramref name="local"/></returns>
        /// <exception cref="ModuleNotLoadedException">Thrown when the given local's owning function's
        /// is not loaded.</exception>
        /// <exception cref="UnboundLocalException"/>Thrown when the given local's owning function is not
        /// the currently executing function.</exception>
        public ZSObject Get(IR.VM.Local local)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the object holding the state of the given module.
        /// 
        /// The module must be loaded into the runtime.
        /// Otherwise, a <see cref="IRObjectNotLoadedException{Module}"/> will be thrown.
        /// </summary>
        /// <param name="module">The IR of the module to return the state object of.</param>
        /// <returns>A <see cref="ZSModule"/> object holding the state of the given module.</returns>
        /// <exception cref="IRObjectNotLoadedException{Module}">Thrown when the given module is not loaded.</exception>
        public ZSModule Get(Module module)
        {
            if (!TryGet(module, out ZSModule? zsModule))
                throw new IRObjectNotLoadedException<Module>(module);

            return zsModule;
        }

        /// <summary>
        /// Returns the value of the given parameter.
        /// 
        /// The parameter's owning function must be the currently executing function.
        /// Otherwise, a <see cref="UnboundParameterException"/> will be thrown.
        /// 
        /// The parameter's owning module - if it has one - must be loaded into the runtime.
        /// Otherwise, a <see cref="ModuleNotLoadedException"/> will be thrown.
        /// </summary>
        /// <param name="parameter">The IR of the parameter to get.</param>
        /// <returns>The value associated with the given <paramref name="parameter"/>.</returns>
        /// <exception cref="ModuleNotLoadedException">Thrown when the given parameter's owning function's
        /// is not loaded.</exception>
        /// <exception cref="UnboundParameterException"/>Thrown when the given parameter's owning function is not
        /// the currently executing function.</exception>
        public ZSObject Get(Parameter parameter)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the object that was previously bound to the given IR object.
        /// 
        /// The IR must be already loaded into the runtime, otherwise a 
        /// <see cref="IRObjectNotLoadedException"/> is thrown.
        /// </summary>
        /// <param name="ir">The IR of which the requested object is bound to.</param>
        /// <returns>The object which was bound to the given <paramref name="ir"/> when
        /// loading the IR into the runtime.</returns>
        /// <exception cref="IRObjectNotLoadedException">Thrown when the given IR object is not 
        /// loaded into the runtime.</exception>
        public ZSObject Get(IRObject ir)
            => TryGet(ir, out ZSIRObject? result) ? result : throw new IRObjectNotLoadedException(ir);
    }
}
