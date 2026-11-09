// std:dotnet — .NET assemblies, imported as files.
//
// Importing it makes a `.dll` file importable by its path, as a `.zs` file is: the
// assembly is loaded into the running platform, unless it is loaded already, as the
// platform's own are, and what importing it gives is its module, whose members are its
// public types, by their names.

import { String, Path, DotNet_Module, Assembly, AppDomain, ilLoader } from "core:runtime";
import { Instance } from "std:zsharp/hir";
import { Error, Result, IResult } from "std:zsharp/common";
import { files } from "core:zsharp";

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
