import { input, print } from "std:io";
import { 
	Console, 
	List, 
	TestClass, 
	TestInterface, 
	greet, 
	id 
} from "net:ZLoad.Test.dll";

module Program;

class MyClass : TestInterface {
	new(this) {}

	fun do(this): void {
		print("Hello from MyClass");

		return;
	}

	fun do(this, x: i32): i32 {
		return x + 1;
	}
}

class OtherClass : TestInterface {
	var x: i32;

	new(this, number: i32) {
		this.x = number;

	}

	fun do(this): void {
		print("Hello from OtherClass");
		return;
	}

	fun do(this, x: i32): i32 {
		return x + this.x;
	}
}

fun testInterface(test: TestInterface): void {

	test.do();
	print(string(test.do(number)));
	return;
}

fun main(): void {
	let c1 = MyClass();
	let c2 = OtherClass(5);

	testInterface(c1);
	testInterface(c2);

	let tc = TestClass();
	print(tc.id[string]("Id<T> [T: string] of TestClass"));

	return;
}

let number = 10;
