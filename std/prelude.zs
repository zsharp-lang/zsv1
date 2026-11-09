// The prelude: what a script imports first, so it gets the standard definitions
// without the toolchain hardcoding any of them. Importing a library installs what it
// defines, as std:functions makes `fun` a Z# function. A library says what its own
// definitions are where it needs another meta-type than the default.

import { Function } from "std:functions";
import { Implementation } from "std:traits";
import "std:traits/static";
import "std:traits/dynamic";

// What the platform can do, and what the CLR runtime has. These come before std:oop:
// it matches a target against Polymorphism, which the dynamic trait dispatcher only
// does for a trait that has an implementation when the code is compiled.
import { Polymorphism } from "std:zsharp/capabilities";
import "std:zsharp/clr";

// Z#'s own modules and classes: they make `module` a Z# module and `def` a Z# class.
import { Module } from "std:modules";
import { Class, Interface } from "std:oop";
import { Type } from "std:types";
