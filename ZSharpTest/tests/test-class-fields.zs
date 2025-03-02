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

	new Old(this) {
		MyClass(this: this, 80);
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
	let old = MyClass.Old();

	MyClass.printAge(old);

	print(string(myClass.age));

	print(string(myClass.age = 20));

	print("myClass.age = " + string(myClass.age));

	MyClass.printAge(myClass);

	return;
}
