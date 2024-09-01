// import { print as output } from "std:io";

module Lib {
	fun bar(x: string): string {
		return x;
	}
}

module Program;



fun main(): string {
	foo();
}

let x = y;
let y = "Hello";

fun foo(): string {
	return Lib.bar(x);
}



/*
if (isProduction)
	fun foo() { }
else
	import { foo } from "foo";
*/