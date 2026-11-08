import { String, static } from "core:runtime";
import { export } from "core:zsharp";

@export
class Greetings {
    @static fun hello(): String { return "Hello from an imported document"; }
}

class Hidden {}
