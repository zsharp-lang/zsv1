// std:reflection — the HIR node of what a definition was built as, as code asks for it as
// it runs.
//
// `reflect` is declared, with no code of its own: reflecting is reading what an encoder
// stored, so it is matched to how that was stored, by mapping, as the prelude maps it
// onto std:encoding's `describe`. It takes any object that carries metadata, as a type
// object, a member, or a Z# class's type object, which carries its metaclass's.

import { Object } from "core:runtime";
import { Instance } from "pkg:zsharp/hir";

fun reflect(value: Object): Instance;
