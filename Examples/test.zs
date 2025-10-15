import { print } from "std:io";
import { Void } from "std:types";

print("[CT] Hello, World!");

import { Directory } from "std:fs";

import { CompilerObject } from "core:compiler";

module A {
	print("[CT] Inside module A");

	fun main(): Void {
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
