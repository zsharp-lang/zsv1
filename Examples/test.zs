import { Directory } from "std:fs";
import { print } from "std:io";

print("[CT] Hello, World!");

fun nodeTypeNotImplemented() {}

module A {
	print("[CT] Inside module A");

	fun main() {
		print("[RT] Hello, World!");
	}
}

if (false) {
	print("[CT] Condition is true");
} else {
	print("[CT] Condition is false");
}

A.main();
