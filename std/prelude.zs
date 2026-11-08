// The prelude: what a script imports first, so it gets the standard definitions
// without the toolchain hardcoding any of them. Importing a library installs what it
// defines, as std:functions makes `fun` a Z# function.

import { Function } from "std:functions";
