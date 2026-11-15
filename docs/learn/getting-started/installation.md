# Installation

A release of Z# is an archive for your platform: `zsharp-win-x64.zip`,
`zsharp-linux-x64.tar.gz`, `zsharp-osx-arm64.tar.gz`, and so on. It needs nothing else
installed, not even .NET.

Extract it anywhere, and run its installer from the extracted folder:

```text
install.cmd            on Windows
./install.sh           on Linux and macOS
```

The installer copies Z# into your own programs, `%LOCALAPPDATA%\Programs\ZSharp` on
Windows and `~/.local/share/zsharp` elsewhere, and puts its `zs` on your PATH, so a new
terminal has it. To choose the folder, give it:

```text
install.cmd D:\Tools\ZSharp
./install.sh ~/tools/zsharp
```

- `--no-path` installs without changing your PATH. Run `<folder>/bin/zs` then.
- `--uninstall` removes Z#, and what installing it put on your PATH. Run it from the
  installed folder, or give the folder: `install.cmd --uninstall D:\Tools\ZSharp`.

The installer replaces or removes a folder only if it is a Z# toolchain, so a folder of
anything else is never written over. Installing into a folder that already has Z# replaces
it, which is how you upgrade.

On Windows, your PATH is your user's `Path` variable. Elsewhere, it is a few marked lines
in your shell's profiles (`~/.profile`, `~/.bashrc`, `~/.zshrc`), which uninstalling
removes.
