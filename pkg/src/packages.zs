// Packages are found in roots, in their order: the project's own, `packages` where the
// toolchain runs from, and then the toolchain's, `packages` next to the standard
// library. More roots can be added. What none of them has is looked for in the packages
// NuGet installed.
//
// In a root, a package named `a/b` is a file, `a/b.dll` or `a/b/b.dll`, or a document,
// `a/b.zs` or `a/b/b.zs`, which are imported as files are, or a project, a directory
// `a/b` whose module file is in it or in its `src`. A project built as an assembly, into
// `a/b/build/b.dll`, is that assembly, loaded from its bytes so it can be built again
// while it is in use; one that isn't is compiled by what can compile one, which is given
// to the package manager. In NuGet's packages, `Name` is the
// newest version installed, and `Name@1.2.3` that version, of the assembly of the target
// framework most like the running one's.

// The project's packages, then the toolchain's, which are next to the standard library.
fun initialRoots(): List[String] {
    let found = List[String]();
    found.Add(Path.Combine(Directory.GetCurrentDirectory(), "packages"));

    let directory = AppContext.BaseDirectory;
    while (directory != null) {
        if (Directory.Exists(Path.Combine(directory, "std"))) {
            let toolchain = Path.Combine(directory, "packages");
            if (found.Contains(toolchain) == false) found.Add(toolchain);
            return found;
        }
        directory = Path.GetDirectoryName(directory);
    }
    return found;
}

// A global's initial value is code that runs where the global is, so what it calls is
// above it.
let roots: List[String] = initialRoots();
let nuget: String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
let projects: Func[String, IResult[Instance, Error]] = null;

// The folders of the built packages loaded, where what they need was put.
let bundled: List[String] = List[String]();

// Adds a root, after those there are.
fun add(root: String) { roots.Add(Path.GetFullPath(root)); }

// Gives what compiles a package that is a project.
fun compileProjectsWith(compiler: Func[String, IResult[Instance, Error]]) { projects = compiler; }

// Imports a package, as `pkg:` does.
fun load(source: String): IResult[Instance, Error] {
    let name = source;
    let version: String = null;
    let at = source.IndexOf("@");
    if (at >= 0) {
        name = source.Substring(0, at);
        version = source.Substring(at + 1);
    }

    if (version == null) {
        let index = 0;
        while (index < roots.Count) {
            let found = inRoot(roots[index], name);
            if (found != null) return found;
            index = index + 1;
        }
    }

    let assembly = inNuGet(name, version);
    if (assembly != null) return Importer(assembly);

    let looked = "";
    let index = 0;
    while (index < roots.Count) {
        looked = looked + roots[index] + ", ";
        index = index + 1;
    }
    return Result[Instance].Error("There is no package " + source + " in " + looked + "or in NuGet's packages at " + nuget + ".");
}

// A package in a root, or null if the root has none of that name.
fun inRoot(root: String, name: String): IResult[Instance, Error] {
    let path = Path.Combine(root, name);
    let last = Path.GetFileName(name);

    if (File.Exists(path + ".dll")) return Importer(path + ".dll");
    if (File.Exists(Path.Combine(path, last + ".dll"))) return Importer(Path.Combine(path, last + ".dll"));
    if (File.Exists(path + ".zs")) return Importer(path + ".zs");
    if (File.Exists(Path.Combine(path, last + ".zs"))) return Importer(Path.Combine(path, last + ".zs"));

    let project: String = null;
    if (File.Exists(Path.Combine(path, ".zs"))) project = path;
    else if (File.Exists(Path.Combine(path, "src", ".zs"))) project = Path.Combine(path, "src");

    let built = Path.Combine(path, "build", last + ".dll");
    if (project != null) {
        if (File.Exists(built)) {
            let folder = Path.GetDirectoryName(built);
            if (bundled.Contains(folder) == false) bundled.Add(folder);
            return Result[Instance].Ok(ilLoader.Load(Assembly.Load(File.ReadAllBytes(built)).ManifestModule));
        }
    }

    if (project != null) {
        if (projects == null) return Result[Instance].Error("The package " + name + " is a project, and nothing compiles projects for the package manager.");
        return projects(project);
    }
    return null;
}

// The assembly of a package NuGet installed, or null.
fun inNuGet(name: String, version: String): String {
    let package = Path.Combine(nuget, name.ToLowerInvariant());
    if (Directory.Exists(package) == false) return null;

    let chosen = version;
    if (chosen == null) chosen = newest(package);
    if (chosen == null) return null;

    let lib = Path.Combine(package, chosen, "lib");
    if (Directory.Exists(lib) == false) return null;

    let frameworks = List[String]();
    frameworks.Add("net10.0");
    frameworks.Add("net9.0");
    frameworks.Add("net8.0");
    frameworks.Add("net7.0");
    frameworks.Add("net6.0");
    frameworks.Add("net5.0");
    frameworks.Add("netcoreapp3.1");
    frameworks.Add("netstandard2.1");
    frameworks.Add("netstandard2.0");

    let index = 0;
    while (index < frameworks.Count) {
        let directory = Path.Combine(lib, frameworks[index]);
        if (Directory.Exists(directory)) {
            let named = Path.Combine(directory, name + ".dll");
            if (File.Exists(named)) return named;
            let any = Directory.GetFiles(directory, "*.dll");
            if (any.Length > 0) return any[0];
        }
        index = index + 1;
    }
    return null;
}

// The assembly of a package an assembly refers to, which the running .NET asks for when
// it has none of that name, as an assembly built against a package refers to it by its
// name and version: the one its build put next to it, or else the version NuGet
// installed, or else the newest one.
fun resolveAssembly(context: AssemblyLoadContext, name: AssemblyName): Assembly {
    let index = 0;
    while (index < bundled.Count) {
        let copy = Path.Combine(bundled[index], name.Name + ".dll");
        if (File.Exists(copy)) return context.LoadFromAssemblyPath(copy);
        index = index + 1;
    }

    let found: String = null;
    if (name.Version != null) found = inNuGet(name.Name, name.Version.Major + "." + name.Version.Minor + "." + name.Version.Build);
    if (found == null) found = inNuGet(name.Name, null);
    if (found == null) return null;
    return context.LoadFromAssemblyPath(found);
}

// The newest version of a package that isn't a preview.
fun newest(package: String): String {
    let best: Version = null;
    let chosen: String = null;
    let versions = Directory.GetDirectories(package);
    let index = 0;
    while (index < versions.Length) {
        let name = Path.GetFileName(versions[index]);
        if (name.Contains("-") == false) {
            let parsed = Version.Parse(name);
            if (best == null or parsed.CompareTo(best) > 0) {
                best = parsed;
                chosen = name;
            }
        }
        index = index + 1;
    }
    return chosen;
}
