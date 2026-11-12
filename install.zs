// Installs the toolchain this is part of, and puts its `zs` on the PATH: `install.cmd`
// on Windows, or `./install.sh` elsewhere, which run this with the release's own `zs`.
//
//   install [folder] [--no-path]   installs into the folder, or else into the user's own
//                                  programs, and adds its `bin` to the user's PATH
//   install --uninstall [folder]   removes the toolchain, this one or the folder's, and
//                [--no-path]       what installing it put on the PATH, unless told not to
//
// A folder is replaced or removed only if it is a toolchain, as the file a release has at
// its root says, so a folder of anything else is never written over. On Windows the PATH
// is the user's own, which a new terminal sees; elsewhere it is a few marked lines in the
// shell's profiles, which a new shell runs.

import "std:prelude";
import { print } from "std:io";
import { arguments } from "core:zsharp";
import { Object, String, Type, Int32, Boolean, List, Path, File, Directory, Environment } from "core:runtime";

let objectAssembly = Object.Assembly.Location;
let runtimeFolder = Path.GetDirectoryName(objectAssembly);
import { AppContext, OperatingSystem, EnvironmentVariableTarget, StringComparison, StringSplitOptions } from objectAssembly;
import { Process, ProcessStartInfo } from Path.Combine(runtimeFolder, "System.Diagnostics.Process.dll");

module Installer {
    // What a release has at its root, which says a folder is a toolchain.
    let marker = ".zsharp-toolchain";

    // What marks the lines an install puts in a shell's profile.
    let begin = "# >>> Z# toolchain >>>";
    let end = "# <<< Z# toolchain <<<";

    // The release this is part of: the folder its `zs` is in is its `bin`.
    fun release(): String { return Path.GetDirectoryName(Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory)); }

    // Where a toolchain goes unless told: the user's own programs.
    fun defaultFolder(): String {
        if (OperatingSystem.IsWindows()) return Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA"), "Programs", "ZSharp");
        let data = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
        if (data == null) data = Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".local", "share");
        return Path.Combine(data, "zsharp");
    }

    fun isToolchain(folder: String): Boolean { return File.Exists(Path.Combine(folder, marker)); }

    fun same(first: String, second: String): Boolean {
        let one = Path.TrimEndingDirectorySeparator(Path.GetFullPath(first));
        let other = Path.TrimEndingDirectorySeparator(Path.GetFullPath(second));
        if (OperatingSystem.IsWindows()) return String.Equals(one, other, StringComparison.OrdinalIgnoreCase);
        return one == other;
    }

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
            Installer.copy(folders[index], Path.Combine(destination, Path.GetFileName(folders[index])));
            index = index + 1;
        }
    }

    // ---- The PATH, on Windows: the user's own

    fun windowsPath(bin: String, adding: Boolean) {
        let path = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User);
        if (path == null) path = "";

        let kept = List[String]();
        let entries = path.Split(";", StringSplitOptions.RemoveEmptyEntries);
        let index = 0;
        while (index < entries.Length) {
            if (Installer.same(entries[index], bin) == false) kept.Add(entries[index]);
            index = index + 1;
        }
        if (adding) kept.Add(bin);

        // The PATH is written only if it changes, so nothing else in it is touched.
        let changed = String.Join(";", kept);
        if (changed != path) Environment.SetEnvironmentVariable("Path", changed, EnvironmentVariableTarget.User);
    }

    // ---- The PATH, elsewhere: marked lines in the shell's profiles

    // A profile without what an install put there.
    fun withoutBlock(text: String): String {
        let start = text.IndexOf(begin);
        if (start < 0) return text;
        let stop = text.IndexOf(end, start);
        if (stop < 0) return text;
        let after = stop + end.Length;
        if (after < text.Length) {
            if (text.Substring(after, 1) == "\n") after = after + 1;
        }
        return text.Substring(0, start) + text.Substring(after);
    }

    fun unixPath(bin: String, adding: Boolean) {
        let home = Environment.GetEnvironmentVariable("HOME");
        let profiles = List[String]();
        profiles.Add(Path.Combine(home, ".profile"));
        profiles.Add(Path.Combine(home, ".bashrc"));
        profiles.Add(Path.Combine(home, ".zshrc"));

        let written = false;
        let index = 0;
        while (index < profiles.Count) {
            let profile = profiles[index];
            if (File.Exists(profile)) {
                let original = File.ReadAllText(profile);
                let text = Installer.withoutBlock(original);
                if (adding) {
                    if (text.Length > 0) {
                        if (text.EndsWith("\n") == false) text = text + "\n";
                    }
                    text = text + begin + "\nexport PATH=\"" + bin + ":$PATH\"\n" + end + "\n";
                }
                if (text != original) File.WriteAllText(profile, text);
                written = true;
            }
            index = index + 1;
        }

        // A user with no profile gets one, which a login shell runs.
        if (adding) {
            if (written == false) File.WriteAllText(profiles[0], begin + "\nexport PATH=\"" + bin + ":$PATH\"\n" + end + "\n");
        }
    }

    fun path(folder: String, adding: Boolean) {
        let bin = Path.Combine(folder, "bin");
        if (OperatingSystem.IsWindows()) Installer.windowsPath(bin, adding);
        else Installer.unixPath(bin, adding);
    }

    // ---- Installing and removing

    fun install(folder: String, onPath: Boolean): Int32 {
        let source = Installer.release();
        if (Installer.isToolchain(source) == false) {
            print("This isn't a release of the toolchain: " + source + " has no " + marker + ".");
            return 1;
        }

        if (Installer.same(source, folder) == false) {
            if (Directory.Exists(folder)) {
                let empty = Directory.GetFileSystemEntries(folder).Length == 0;
                if (empty == false) {
                    if (Installer.isToolchain(folder) == false) {
                        print(folder + " isn't empty, and isn't a Z# toolchain, so nothing was written to it.");
                        return 1;
                    }
                    print("Replacing the toolchain in " + folder + ".");
                    Directory.Delete(folder, true);
                }
            }
            // What runs as a program keeps its mode, as the release's archive has it.
            Installer.copy(source, folder);
        }

        if (onPath) Installer.path(folder, true);

        print("Installed the Z# toolchain in " + folder + ".");
        if (onPath) print("Its zs is on your PATH in a new terminal: zs <script.zs>.");
        else print("Run it as " + Path.Combine(folder, "bin", "zs") + " <script.zs>.");
        return 0;
    }

    fun uninstall(folder: String, onPath: Boolean): Int32 {
        if (Installer.isToolchain(folder) == false) {
            print(folder + " isn't a Z# toolchain, so nothing was removed.");
            return 1;
        }

        if (onPath) Installer.path(folder, false);
        let fromPath = "";
        if (onPath) fromPath = ", and from your PATH";

        // The `zs` running this is in the folder it removes, which can't go while it runs:
        // on Windows its files can't be removed, and elsewhere what it hasn't loaded yet
        // would be gone. So what removes the folder waits for it to exit.
        if (Installer.same(folder, Installer.release())) {
            let start = ProcessStartInfo();
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            if (OperatingSystem.IsWindows()) {
                start.FileName = "cmd.exe";
                start.Arguments = "/c ping -n 3 127.0.0.1 >nul & rmdir /s /q \"" + folder + "\"";
            } else {
                start.FileName = "/bin/sh";
                start.ArgumentList.Add("-c");
                start.ArgumentList.Add("sleep 2; rm -rf \"$0\"");
                start.ArgumentList.Add(folder);
            }
            print("Removed the Z# toolchain" + fromPath + "; its folder, " + folder + ", is removed once this exits.");
            Process.Start(start);
            return 0;
        }

        print("Removed the Z# toolchain from " + folder + fromPath + ".");
        Directory.Delete(folder, true);
        return 0;
    }
}

let folder: String = null;
let onPath = true;
let removing = false;
let index = 0;
while (index < arguments.Count) {
    let given = arguments[index];
    if (given == "--no-path") onPath = false;
    else if (given == "--uninstall") removing = true;
    else folder = given;
    index = index + 1;
}

if (removing) {
    if (folder == null) folder = Installer.release();
    Environment.Exit(Installer.uninstall(Path.GetFullPath(folder), onPath));
}

if (folder == null) folder = Installer.defaultFolder();
Environment.Exit(Installer.install(Path.GetFullPath(folder), onPath));
