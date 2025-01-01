import { print as output } from "std:io";

module Lib {
	fun bar(x: string = "hello"): string {
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

	new(self: Foo)
	{

	}

	fun bar(this): i32 {
		output("bar");

		// zzz(this);

		let blah = Blah();
		blah.v = "zzz arg";

		this.zzz(v);

		return exitCode;
	}

	fun zzz(this, x: class Blah { let v: string = ""; }): void {
		output(this.x = x.v);

		return;
	}
}

/*
if (isProduction)
	fun foo() { }
else
	import { foo } from "foo";
*/