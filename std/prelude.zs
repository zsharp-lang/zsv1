// The prelude: what a script imports first, so it gets the standard definitions
// without the toolchain hardcoding any of them. Importing a library installs what it
// defines, as std:functions makes `fun` a Z# function, by importing what compiles it. A
// library says what its own definitions are where it needs another meta-type than the
// default.

// A library that is built is imported as what its build describes, which std:encoding
// decodes; and, as what it gives is a Z# module, std:modules comes next.
import "std:encoding";
import { Module } from "std:modules";
import { Function } from "std:functions";
import { Implementation } from "std:traits";
import "std:traits/static";
import "std:traits/dynamic";

// What the platform can do, and what the CLR runtime has. These come before std:oop:
// it matches a target against Polymorphism, which the dynamic trait dispatcher only
// does for a trait that has an implementation when the code is compiled.
import { Polymorphism } from "std:zsharp/capabilities";
import "std:zsharp/clr";

// Z#'s own classes: they make `def` a Z# class.
import { Class, Interface } from "std:oop";
import { Type } from "std:types";

// Reflection, as std:encoding gives again what it encoded, and as the IL loader describes
// what carries nothing it encoded.
import { reflect } from "std:reflection";
import { premap } from "std:functions/compiler";
import { describe } from "std:encoding";
import { Instance } from "std:zsharp/hir";
premap(reflect as Instance, describe as Instance);

// .NET assemblies as files, and packages, as `pkg:`, from the package manager.
import "std:dotnet";
import "std:packages";
