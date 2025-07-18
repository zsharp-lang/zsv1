import { input, print } from "std:io";
import { 
	BaseClass,
	Console, 
	List, 
	TestClass, 
	TestEnum,
	TestInterface, 
	greet, 
	id 
} from "net:ZLoad.Test.dll";
import { Directory, File } from "net:ZSharp.CT.StandardLibrary.FileSystem.dll";

import { Expression, LiteralType } from "net:ZSharp.AST.dll";

print("Hello, Document!");

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

class Base {
	fun baseMethod(this): void {
		print("Base::baseMethod");

		return;
	}
}

class Derived : Base {

	new() {}

	fun derivedMethod(this): void {
		print("Derived::derivedMethod");

		return;
	}
}

fun testInterface(test: TestInterface): void {

	test.do();
	print(string(test.do(number)));
	return;
}

fun testBaseClass(test: BaseClass): void {
	test.doVirtual();
	return;
}

fun main(): void {
	
	let c1 = MyClass();
	let c2 = OtherClass(5);

	testInterface(c1);
	testInterface(c2);

	let tc = TestClass();
	print(tc.id[string]("Id<T> [T: string] of TestClass"));

	testBaseClass(BaseClass());
	testBaseClass(tc);

	let cwd = Directory.cwd();
	print(cwd.toString());
	let testsDirectory = cwd / "tests" as Directory;
	let filePath = testsDirectory / "simple.zs";
	print(filePath.toString());
	//print(filePath.asFile().toString());
	print(id("Id<T> [T: string]"));
	
	
	let base: Base = Derived();
	base.baseMethod();

	//if (base is derived of Derived)
	//	derived.derivedMethod();

	let derived = base as Derived;
	derived.derivedMethod();

	if (filePath is simpleFilePath of File)
		print(simpleFilePath.getContent());

	let testDirectory = testsDirectory.createDirectory("test", existsOk: true);

	//testsDirectory.createDirectory("test", existsOk: false);

	let testEnum = TestEnum.C;
	print(string(testEnum));

	print(string(LiteralType.False));
	
	return;
}

let number = 10;
