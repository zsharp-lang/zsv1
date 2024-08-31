// import { print as output } from "std:io";

module Lib {
	fun bar(x: string): string {
		return x;
	}
}

module Program;

	let y = "Hello";
let x = y;

fun foo(): string {
	return Lib.bar(x);
}

fun main(): string {
	foo();
}



/*
if (isProduction)
	fun foo() { }
else
	import { foo } from "foo";
*/