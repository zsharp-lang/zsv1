// Builds a release of the toolchain: what an install has, for a platform, as an archive
// that needs nothing else installed, in `release`. `zs release.zs [--no-bootstrap]
// [platform...]`, each platform a .NET runtime identifier, as win-x64, linux-x64 or
// osx-arm64, or else the one this runs on. Run it from the toolchain's directory.
//
// The toolchain is published self-contained, so the archive runs where .NET isn't
// installed. With it go the sources and builds of the standard library, which are its
// seed, the toolchain's packages, the bootstrap, so an install can build itself again
// from its sources, the documentation and examples, and the installer. The bootstrap runs
// first, unless told not to, so the seed is what the sources build.

import "std:prelude";
import { print } from "std:io";
import { arguments } from "core:zsharp";
import { inNuGet } from "core:packages";
import { Object, String, Type, Int32, Boolean, List, Path, File, Directory, Environment } from "core:runtime";

let objectAssembly = Object.Assembly.Location;
let runtimeFolder = Path.GetDirectoryName(objectAssembly);
import { AppContext, RuntimeInformation, DateTime, SearchOption } from objectAssembly;
import { Process, ProcessStartInfo } from Path.Combine(runtimeFolder, "System.Diagnostics.Process.dll");
import { ZipFile } from Path.Combine(runtimeFolder, "System.IO.Compression.ZipFile.dll");
import { GZipStream, CompressionLevel } from Path.Combine(runtimeFolder, "System.IO.Compression.dll");
import { TarWriter, PaxTarEntry, TarEntryType, TarEntryFormat } from Path.Combine(runtimeFolder, "System.Formats.Tar.dll");
import { UnixFileMode } from objectAssembly;

module Release {
    // The toolchain's directory: the one the standard library is in, at or above where
    // this runs from.
    fun root(): String {
        let found = Path.GetFullPath(".");
        while (Directory.Exists(Path.Combine(found, "std")) == false) found = Path.GetDirectoryName(found);
        return found;
    }

    // Runs a program in a directory, and gives what it exits with.
    fun run(file: String, given: List[String], directory: String): Int32 {
        let start = ProcessStartInfo();
        start.FileName = file;
        let index = 0;
        while (index < given.Count) {
            start.ArgumentList.Add(given[index]);
            index = index + 1;
        }
        start.WorkingDirectory = directory;
        start.UseShellExecute = false;

        let process = Process.Start(start);
        process.WaitForExit();
        return process.ExitCode;
    }

    // Copies a directory into another, but for what building leaves of C#.
    fun copy(source: String, destination: String) {
        Directory.CreateDirectory(destination);

        let files = Directory.GetFiles(source);
        let index = 0;
        while (index < files.Length) {
            File.Copy(files[index], Path.Combine(destination, Path.GetFileName(files[index])), true);
            index = index + 1;
        }

        let folders = Directory.GetDirectories(source);
        index = 0;
        while (index < folders.Length) {
            let name = Path.GetFileName(folders[index]);
            if (name != "obj") {
                if (name != "bin") Release.copy(folders[index], Path.Combine(destination, name));
            }
            index = index + 1;
        }
    }

    // Whether an archive's file runs as a program, which a tarball says by its mode.
    fun executable(name: String): Boolean {
        if (name == "bin/zs") return true;
        if (name == "bin/createdump") return true;
        if (name == "install.sh") return true;
        return false;
    }

    // Writes a directory's files into a tarball, under a folder of its name, with what
    // each may do on a platform that keeps it.
    fun entries(writer: TarWriter, source: String, prefix: String, relative: String) {
        let readable = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.GroupRead | UnixFileMode.OtherRead;
        let runnable = readable | UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute;

        let folder = PaxTarEntry(TarEntryType.Directory, prefix + relative);
        folder.Mode = runnable;
        writer.WriteEntry(folder);

        let files = Directory.GetFiles(source);
        let index = 0;
        while (index < files.Length) {
            let name = relative + Path.GetFileName(files[index]);
            let entry = PaxTarEntry(TarEntryType.RegularFile, prefix + name);
            entry.Mode = readable;
            if (Release.executable(name)) entry.Mode = runnable;
            let data = File.OpenRead(files[index]);
            entry.DataStream = data;
            writer.WriteEntry(entry);
            data.Dispose();
            index = index + 1;
        }

        let folders = Directory.GetDirectories(source);
        index = 0;
        while (index < folders.Length) {
            Release.entries(writer, folders[index], prefix, relative + Path.GetFileName(folders[index]) + "/");
            index = index + 1;
        }
    }

    fun tarball(source: String, archive: String) {
        let output = File.Create(archive);
        let compressed = GZipStream(output, CompressionLevel.Optimal);
        let writer = TarWriter(compressed, TarEntryFormat.Pax, false);
        Release.entries(writer, source, Path.GetFileName(source) + "/", "");
        writer.Dispose();
        compressed.Dispose();
        output.Dispose();
    }

    // The versioned packages the toolchain's own packages import, as `pkg:Name@1.2.3`,
    // which a release carries, since an install has no NuGet packages of its own.
    fun versioned(folder: String, found: List[String]) {
        let files = Directory.GetFiles(folder, "*.zs", SearchOption.AllDirectories);
        let index = 0;
        while (index < files.Length) {
            let text = File.ReadAllText(files[index]);
            let at = text.IndexOf("\"pkg:");
            while (at >= 0) {
                let stop = text.IndexOf("\"", at + 1);
                if (stop < 0) at = -1;
                else {
                    let package = text.Substring(at + 5, stop - at - 5);
                    if (package.Contains("@")) {
                        if (found.Contains(package) == false) found.Add(package);
                    }
                    at = text.IndexOf("\"pkg:", stop + 1);
                }
            }
            index = index + 1;
        }
    }

    // Copies a version of a package NuGet installed into a release's packages, as the
    // package manager finds it there, `Name@1.2.3/Name.dll`: the assemblies the package
    // manager would import of it, and those next to them. Gives whether it was there.
    fun carry(package: String, packages: String): Boolean {
        let at = package.IndexOf("@");
        let assembly = inNuGet(package.Substring(0, at), package.Substring(at + 1));
        if (assembly == null) return false;

        let destination = Path.Combine(packages, package);
        Directory.CreateDirectory(destination);
        let files = Directory.GetFiles(Path.GetDirectoryName(assembly), "*.dll");
        let index = 0;
        while (index < files.Length) {
            File.Copy(files[index], Path.Combine(destination, Path.GetFileName(files[index])), true);
            index = index + 1;
        }
        return true;
    }

    // Builds the release for a platform, and gives where its archive is, or null.
    fun build(platform: String): String {
        let root = Release.root();
        let releases = Path.Combine(root, "release");
        let name = "zsharp-" + platform;
        let stage = Path.Combine(releases, name);
        if (Directory.Exists(stage)) Directory.Delete(stage, true);
        Directory.CreateDirectory(stage);

        print("Publishing the toolchain for " + platform + ".");
        let publish = List[String]();
        publish.Add("publish");
        publish.Add(Path.Combine(root, "src", "CLI", "ZSharp.CLI", "ZSharp.CLI.csproj"));
        publish.Add("-c");
        publish.Add("Release");
        publish.Add("-r");
        publish.Add(platform);
        publish.Add("--self-contained");
        publish.Add("true");
        publish.Add("-o");
        publish.Add(Path.Combine(stage, "bin"));
        publish.Add("--nologo");
        publish.Add("-v");
        publish.Add("q");
        if (Release.run("dotnet", publish, root) != 0) {
            print("Publishing for " + platform + " failed.");
            return null;
        }

        let folders = List[String]();
        folders.Add("std");
        folders.Add("packages");
        folders.Add("examples");
        folders.Add("design");
        folders.Add("docs");
        let index = 0;
        while (index < folders.Count) {
            let folder = Path.Combine(root, folders[index]);
            if (Directory.Exists(folder)) Release.copy(folder, Path.Combine(stage, folders[index]));
            index = index + 1;
        }

        let wanted = List[String]();
        Release.versioned(Path.Combine(root, "packages"), wanted);
        index = 0;
        while (index < wanted.Count) {
            if (Release.carry(wanted[index], Path.Combine(stage, "packages")) == false) {
                print("The package " + wanted[index] + " the toolchain's packages import isn't in NuGet's packages, so there is no release.");
                return null;
            }
            index = index + 1;
        }

        let files = List[String]();
        files.Add("bootstrap.zs");
        files.Add("install.zs");
        files.Add("install.cmd");
        files.Add("install.sh");
        index = 0;
        while (index < files.Count) {
            File.Copy(Path.Combine(root, files[index]), Path.Combine(stage, files[index]), true);
            index = index + 1;
        }

        // What says the folder is the toolchain, which the installer replaces and removes
        // only where it is.
        File.WriteAllText(Path.Combine(stage, ".zsharp-toolchain"), "Z# toolchain for " + platform + ", released " + DateTime.UtcNow.ToString("yyyy-MM-dd") + ".\n");

        let archive = Path.Combine(releases, name + ".tar.gz");
        if (platform.StartsWith("win")) archive = Path.Combine(releases, name + ".zip");
        if (File.Exists(archive)) File.Delete(archive);

        if (platform.StartsWith("win")) ZipFile.CreateFromDirectory(stage, archive, CompressionLevel.Optimal, true);
        else Release.tarball(stage, archive);
        return archive;
    }
}

let platforms = List[String]();
let bootstrapFirst = true;
let index = 0;
while (index < arguments.Count) {
    if (arguments[index] == "--no-bootstrap") bootstrapFirst = false;
    else platforms.Add(arguments[index]);
    index = index + 1;
}
if (platforms.Count == 0) platforms.Add(RuntimeInformation.RuntimeIdentifier);

// The seed is what the sources build, so the release is too.
if (bootstrapFirst) {
    print("Bootstrapping, so the seed is what the sources build.");
    let bootstrap = List[String]();
    if (Path.GetFileNameWithoutExtension(Environment.ProcessPath) == "dotnet") bootstrap.Add(Path.Combine(AppContext.BaseDirectory, "zs.dll"));
    bootstrap.Add(Path.Combine(Release.root(), "bootstrap.zs"));
    if (Release.run(Environment.ProcessPath, bootstrap, Release.root()) != 0) {
        print("The bootstrap failed, so there is no release.");
        Environment.Exit(1);
    }
}

index = 0;
while (index < platforms.Count) {
    let archive = Release.build(platforms[index]);
    if (archive == null) Environment.Exit(1);
    print("Wrote " + archive + ".");
    index = index + 1;
}
