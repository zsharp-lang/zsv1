# The bootstrap and releases

## The seed

The standard library is written in Z#, so building it needs a standard library: the one
built before. The builds of the standard library, in `std/*/build`, are its seed, and
every install has one. A library is imported as its build, if it has one, or else
compiled from its sources.

## The bootstrap

`bootstrap.zs` builds the standard library and the toolchain's packages again from their
sources, as a compiler that compiles itself does. Run it from the toolchain's folder:

```text
zs --no-prelude bootstrap.zs
```

It imports the prelude itself, once it has removed what earlier bootstraps built of the
packages, so it runs without the one `zs` would import first.

It runs in generations. Each one builds every library with the libraries it has loaded,
and if any of them built something else than what was loaded, the next generation, a new
process, loads what was built and builds again, until a generation builds what it
loads: a fixpoint.

```text
Generation 1: the Cecil platform builds itself, the same, from its generation 1.
Generation 1: the standard library built something else than it loads.
Generation 2: the Cecil platform builds itself, the same, from its generation 1.
Generation 2: the standard library builds itself, the same.
Bootstrapped.
```

A library is built only by a generation that has loaded what it needs built, so a change
to a library its dependents need reaches them a generation later. An install's sources
build it the same way, so fixing the standard library is changing its sources and running
the bootstrap.

## Releases

`release.zs` builds what an install has, for a platform, as an archive:

```text
zs release.zs [--no-bootstrap] [platform...]
```

Each platform is a .NET runtime identifier, as `win-x64`, `linux-x64` or `osx-arm64`, or
else the one it runs on. It runs the bootstrap first, so the seed is what the sources
build, publishes the toolchain self-contained, and writes `release/zsharp-<platform>.zip`
for Windows and `.tar.gz` elsewhere. With the toolchain go the standard library's
sources and builds, the toolchain's packages and the packages they need, the bootstrap,
the documentation, the examples, and the installer, which
[Getting started](../getting-started.md) describes.
