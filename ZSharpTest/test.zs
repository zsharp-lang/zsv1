// import { print as output } from "std:io";

module Test;
module Lib {
	fun bar() {
		return "Hello";
	}
}


	let y = "Hello";
let x = y;

fun foo(x=x) {
	return Lib.bar();
}



/*
if (isProduction)
	fun foo() { }
else
	import { foo } from "foo";
*/