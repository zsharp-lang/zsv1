import { String } from "core:runtime";
import { static } from "pkg:clr/hir";
import { export } from "core:zsharp";

@export
def Greetings {
    @static fun hello(): String { return "Hello from an imported document"; }
}

def Hidden {}
