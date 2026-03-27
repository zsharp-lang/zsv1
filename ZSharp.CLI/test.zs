import { print } from "std:io";


import { Directory } from "std:fs";

print("[CT] Hello, World!");

fun nodeTypeNotImplemented() {}

module A {
        print("[CT] Inside module A");

        fun main() {
                print("[RT] Hello, World!");

                return;
        }
}

if (false) {
        print("[CT] Condition is true");
} else {
        print("[CT] Condition is false");
}

A.main();
