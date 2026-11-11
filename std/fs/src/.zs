// entrypoint for the submodule.

import { compile, emit } from "core:project";


// everything imported here is also accessible by other files

@emit // the result of compiling this submodule is the HIR of the given module
module FileSystem {

    compile(); // this compiles the rest of the files in the submodule as if they were written here.

    // Since we're compiling inside a module, we get forward referencing for free.
}


// could've also done this (need to verify these are equivalent):

emit(
    module FileSystem {
        compile();
    } as Module // the cast here is so we don't load the module into the runtime
);
