// std:dotnet — .NET assemblies, imported as files.
//
// Importing it makes a `.dll` file importable by its path, as a `.zs` file is: the
// assembly is loaded into the running platform, unless it is loaded already, as the
// platform's own are, and what importing it gives is its module, whose members are its
// public types, by their names.

import { Object, String, Type, Path, File, List, DotNet_Module, Assembly, AppDomain, ilLoader } from "core:runtime";
import { Instance } from "std:zsharp/hir";
import { Error, Result, IResult } from "std:zsharp/common";
import { files, export } from "core:zsharp";

def Assemblies of DotNet_Module {
    fun load(path: String): IResult[Instance, Error] {
        return Result[Instance].Ok(ilLoader.Load(assembly(path).ManifestModule));
    }

    fun assembly(path: String): Assembly {
        let loaded = AppDomain.CurrentDomain.GetAssemblies();
        let index = 0;
        while (index < loaded.Length) {
            // One made by the process, or loaded from bytes, has no file.
            if (loaded[index].IsDynamic == false) {
                if (loaded[index].Location != "") {
                    if (Path.GetFullPath(loaded[index].Location) == path) return loaded[index];
                }
            }
            index = index + 1;
        }
        return Assembly.LoadFrom(path);
    }
}

files.Register(".dll", Assemblies.load);

let objectType: Type = Object;
import { Convert } from objectType.Assembly.Location;

// Copies the assemblies a build needs into the directory it is built into, so it runs
// where they aren't installed. One already there, the same, is left, as it may be in use.
@export
fun bundle(assemblies: List[String], directory: String) {
    let index = 0;
    while (index < assemblies.Count) {
        let copy = Path.Combine(directory, Path.GetFileName(assemblies[index]));
        let same = false;
        if (File.Exists(copy)) same = Convert.ToBase64String(File.ReadAllBytes(copy)) == Convert.ToBase64String(File.ReadAllBytes(assemblies[index]));
        if (same == false) File.Copy(assemblies[index], copy, true);
        index = index + 1;
    }
}
