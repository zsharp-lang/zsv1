import { String } from "core:runtime"; import { static } from "std:zsharp/package/dotnet";
import { export } from "core:zsharp";

@export
def Greetings {
    @static fun hello(): String { return "Hello from an imported document"; }
}

def Hidden {}
