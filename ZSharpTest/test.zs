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
	blah();

	output(pi + 5.0);

	output(1 + 1);

	output(true);
	output(false);

	return foo.bar();
}

let x = y;
let y = "Hello";

let exitCode = 100;

let pi = 3.14;

let foo = Foo();

fun blah(): Lib.id(string) {
	let tmp = Lib.bar(x);

	return tmp;
}

class Foo {
	let x = Lib.bar(y);

	fun bar(this): i32 {
		output("bar");

		// zzz(this);

		this.zzz();

		return exitCode;
	}

	fun zzz(this): void {
		this.x = "this.x!!";

		output(this.x);

		return;
	}
}

/*
if (isProduction)
	fun foo() { }
else
	import { foo } from "foo";
*/