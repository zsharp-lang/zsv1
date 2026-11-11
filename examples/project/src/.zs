// The project's root module file: it emits the Shop module, which is compiled from the
// other files here, and those in text/, which has no module file. Compiling it runs
// greetings/.zs too, as greetings is a submodule.
import { compile, emit } from "core:project";
import { String, Int32 } from "core:runtime";

emit(compile("Shop"));
