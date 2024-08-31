// import { print as output } from "std:io";

module Lib {
	fun bar(): string {
		return "Hello";
	}
}

module Test;

	let y = "Hello";
let x = y;

fun foo(x=x): string {
	return Lib.bar();
}



/*
if (isProduction)
	fun foo() { }
else
	import { foo } from "foo";
*/