# The zs command

```text
zs [--trace-reification] [--dump-definitions] <script.zs> [arguments...]
```

`zs` runs a script, a document of Z#, and exits with 1 if it fails, printing why to
standard error. What follows the script is the script's own, as `core:zsharp`'s
`arguments`. The options before it are for looking into the compiler:

- `--trace-reification` prints the code each reification receives and produces.
- `--dump-definitions` prints the HIR of each definition a document defines.

Both print to standard error, so what the script prints stays as it is.

## Where the standard library is

`zs` finds the standard library in the first folder named `std` at or above its own
folder, so an install is a folder with `bin`, where `zs` is, next to `std`. Nothing in it
names where it is installed, so a toolchain can be moved, or several kept side by side.

An install also has `packages`, the toolchain's packages, as the Cecil platform and the
package manager, and `bootstrap.zs`, which builds the standard library and the packages
again from their sources: [the bootstrap](bootstrap.md).
