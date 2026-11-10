using System.Reflection;
using System.Runtime.Loader;

namespace ZSharp.Importer.ILLoader
{
    /// <summary>
    /// Assemblies imported as modules. The first time one is imported, its modules are
    /// entered: each method it marks as an entry point is called, once, as importing a
    /// module means. This is what loads what the toolchain is built of, as its standard
    /// library, before anything written in Z# can.
    /// </summary>
    public sealed class Assemblies(ILLoader loader)
    {
        private readonly HashSet<Assembly> entered = [];

        /// <summary>
        /// The module of an assembly, which importing it gives, once it is entered.
        /// </summary>
        public Package.DotNet.Module Import(Assembly assembly)
        {
            var module = loader.Load(assembly.ManifestModule);
            Enter(assembly);
            return module;
        }

        /// <summary>
        /// A .dll file, as a file importer imports it: the assembly of that file loaded
        /// already, as the toolchain's own are, or else the file, loaded.
        /// </summary>
        public IResult<HIR.Instance, Error> ImportFile(string path)
        {
            var full = Path.GetFullPath(path);

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                if (!assembly.IsDynamic && assembly.Location != string.Empty && Path.GetFullPath(assembly.Location) == full)
                    return Result<HIR.Instance>.Ok(Import(assembly));

            return Result<HIR.Instance>.Ok(Import(Assembly.LoadFrom(full)));
        }

        /// <summary>
        /// An assembly built where it is in use, as the standard library is: read from its
        /// bytes, so building it again can write its file, once, or the one of its name
        /// loaded already.
        /// </summary>
        public static Assembly Read(string path)
        {
            var name = AssemblyName.GetAssemblyName(path).Name;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                if (assembly.GetName().Name == name)
                    return assembly;

            using var bytes = new MemoryStream(File.ReadAllBytes(path));
            return AssemblyLoadContext.Default.LoadFromStream(bytes);
        }

        /// <summary>
        /// Calls each entry point of the assembly's modules, the first time.
        /// </summary>
        public void Enter(Assembly assembly)
        {
            if (!entered.Add(assembly))
                return;

            foreach (var type in assembly.GetTypes())
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    if (method.IsDefined(typeof(EntryPointAttribute), false))
                        method.Invoke(null, null);
        }
    }
}
