import { print as output } from "std:io";

module Lib {
	fun bar(x: string): string {
		return x;
	}

	fun id(x: type): type {
		output(x);
		return x;
	}
}

module Program;

fun main(): i32 {
	foo();

	output(pi + 5.0);

	output(1 + 1);

	output(true);
	output(false);

	return exitCode;
}

let x = y;
let y = "Hello";

let exitCode = 100;

let pi = 3.14;

fun foo(): Lib.id(string) {
	let tmp = Lib.bar(x);

	return tmp;
}

class Foo {
	let x = Lib.bar(y);
}

/*
if (isProduction)
	fun foo() { }
else
	import { foo } from "foo";
*/