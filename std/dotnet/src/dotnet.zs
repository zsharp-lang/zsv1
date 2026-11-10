// std:dotnet — what a build of a .NET assembly needs.
//
// A `.dll` file is imported by its path, as a `.zs` file is, by the host: the assembly,
// loaded unless it is loaded already, as its module, whose members are its public types.
// The first time an assembly is imported, its modules are entered.

import { Object, String, Type, Path, File, List } from "core:runtime";
import {  } from "core:zsharp";


// Convert is CoreLib's, which the module file imports.

// Copies the assemblies a build needs into the directory it is built into, so it runs
// where they aren't installed. One already there, the same, is left, as it may be in use.
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
