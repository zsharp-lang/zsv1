import { input, print } from "std:io";
import { Console, List, TestInterface, greet, id } from "net:ZLoad.Test.dll";

module Program;

class MyClass : TestInterface {
	new(this) {}

	fun do(this: TestInterface): void {
		print("Hello from MyClass");

		return;
	}
}

class OtherClass : TestInterface {
	new(this) {}

	fun do(this: TestInterface): void {
		print("Hello from OtherClass");
		return;
	}
}

fun testInterface(test: TestInterface): void {
	test.do();
	return;
}

fun main(): void {
	testInterface(MyClass());
	testInterface(OtherClass());

	return;
}
