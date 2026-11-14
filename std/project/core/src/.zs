// std:project/core, as a project: its module, built. Its sources define `compile` and
// `emit` themselves, so the module file's are named otherwise.
import { compile as compileSources, emit as emitModule } from "core:project";
import { build } from "std:framework";
emitModule(build(compileSources("core")));
