// The prelude: what a script imports first, so it gets the standard definitions
// without the toolchain hardcoding any of them. Importing a library installs what it
// defines, as std:functions makes `fun` a Z# function.

import { Function } from "std:functions";
import { Implementation } from "std:traits";
import "std:traits/static";
import "std:traits/dynamic";

// What the platform can do, and what the CLR runtime has.
import { Polymorphism } from "std:zsharp/capabilities";
import "std:zsharp/clr";

// Z#'s own classes. It comes last, as it makes `def` a Z# class, and the libraries
// before it write their CLR classes with `def`.
import { Class, Interface } from "std:oop";
