import { print } from "std:io";

module Program;

class MyBase {
	var name: string;
}

class MyClass : MyBase {
	let age: i32 = 0;

	var base: MyBase;

	new (this, age: i32) {
		base = this;
		this.age = age;
	}

	fun printAge(this): void {
		return MyClass.printAge(this, "this.age = ");
	}

	fun printAge(this, message: string): void {
		print(message + string(this.age));

		return;
	}
}


fun main(): void {
	let myClass = MyClass(15);

	print(string(myClass.age));

	print(string(myClass.age = 20));

	print("myClass.age = " + string(myClass.age));

	MyClass.printAge(myClass);

	return;
}
