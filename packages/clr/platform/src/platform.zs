// pkg:clr/platform — what a build of a .NET assembly needs, on the CLR. It was
// std:dotnet: it is the CLR platform's, not the standard library's.

import { Object, String, Type, List } from "core:runtime";
import { Path, File } from Object.Assembly;
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
