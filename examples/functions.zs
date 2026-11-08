// A script imports the prelude first, which brings the standard definitions:
// here, a fun is a Z# function, defined in Z# by std:functions.
import "std:prelude";
import { print } from "std:io";
import { String, Int32 } from "core:runtime";

fun factorial(n: Int32): Int32 {
    if (n < 2) return 1;
    return n * factorial(n - 1);
}

fun greet(name: String, {punctuation: String = "!"}): String {
    return "Hello, " + name + punctuation;
}

print(greet("Z#"));
print("5! is " + factorial(5));
