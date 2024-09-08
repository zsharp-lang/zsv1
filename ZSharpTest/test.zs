// import { print as output } from "std:io";

module Lib {
	fun bar(x: string): string {
		return x;
	}

	fun id(x: type): type {
		print(x);
		return x;
	}
}

module Program;

fun main(): string {
	foo();

	return "ok";
}

let x = y;
let y = "Hello";

fun foo(): Lib.id(string) {
	let tmp = Lib.bar(x);

	return tmp;
}

class Foo {
	//let x = Lib.bar(y);
}

/*
if (isProduction)
	fun foo() { }
else
	import { foo } from "foo";
*/