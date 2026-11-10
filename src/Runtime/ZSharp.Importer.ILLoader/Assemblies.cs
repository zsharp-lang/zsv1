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
    public sealed class Assemblies(ILLoader loader, IList<Func<Assembly, HIR.Instance?>> describers)
    {
        private readonly HashSet<Assembly> entered = [];

        private readonly Dictionary<Assembly, HIR.Instance> described = [];

        /// <summary>
        /// What imports a library by its source, as the decoder an assembly's blob names,
        /// which the host gives.
        /// </summary>
        public Func<string, object?>? ImportLibrary { get; set; }

        private readonly HashSet<string> decoders = [];

        /// <summary>
        /// What importing an assembly gives, once it is entered: what the first describer
        /// that knows it gives, as one that reads what an encoder wrote of it, or else the
        /// loader's description of its module.
        /// </summary>
        public HIR.Instance Import(Assembly assembly)
        {
            if (!described.TryGetValue(assembly, out var module))
            {
                // What decodes the assembly's blob is imported first, as it describes it,
                // unless it is what is being imported.
                if (Metadata.Decoder(assembly) is { } decoder && ImportLibrary is not null && decoders.Add(decoder))
                    ImportLibrary(decoder);

                // A describer that fails leaves the assembly to the loader, so a library a
                // describer can't read, as a seed one built wrong, still loads.
                foreach (var describer in describers)
                {
                    HIR.Instance? found;

                    try
                    {
                        found = describer(assembly);
                    }
                    catch (Exception failure)
                    {
                        Console.Error.WriteLine($"warning: {assembly.GetName().Name} can't be described: {(failure.InnerException ?? failure).Message}");
                        found = null;
                    }

                    if (found is not null)
                    {
                        module = found;
                        break;
                    }
                }

                described[assembly] = module ??= loader.Load(assembly.ManifestModule);
            }

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
            var read = AssemblyLoadContext.Default.LoadFromStream(bytes);
            files[read] = Path.GetFullPath(path);
            return read;
        }

        private static readonly Dictionary<Assembly, string> files = [];

        /// <summary>
        /// The file an assembly was loaded from, read from its bytes or not, as a platform
        /// that reads what an assembly refers to needs: or <see langword="null"/>, for one
        /// the process made.
        /// </summary>
        public static string? FileOf(Assembly assembly)
            => assembly.IsDynamic ? null
                : assembly.Location != string.Empty ? assembly.Location
                : files.GetValueOrDefault(assembly);

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
