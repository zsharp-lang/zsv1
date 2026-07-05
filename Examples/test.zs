import { print } from "std:io";
import { Void, Test, Object } from "std:types";

print("[CT] Hello, World!");

import { Directory } from "std:fs";

import { CompilerObject } from "core:compiler";

// import { Class as ILClass } from "core:language-extensions";

// class MyCustomCO 
// 	of ILClass.Construct
// 	: CompilerObject
// {

// }

// module A {
// 	print("[CT] Inside module A");

// 	class Foo {
// 		print("[CT] Inside class Foo");
// 	}

// 	// class MyCustomCO 
// 	// 	of ILClass.Construct
// 	// 	: Object
// 	// 	//, CompilerObject
// 	// {
// 	// 	print("[CT] Inside class MyCustomCO");

// 	// 	fun foo(): Void { print("[RT] Inside foo"); return; }

// 	// 	//let x = 32;

// 	// 	new(this: MyCustomCO) {
// 	// 		print("[RT] Inside MyCustomCO constructor");

// 	// 		return;
// 	// 	}
// 	// }

// 	fun main(): Void {
// 		//let cwd = Directory.cwd().toString();

// 		print(Directory.cwd().toString());
// 		print(Test(Directory.cwd().toString()));

// 		print(MyCustomCO());
// 		MyCustomCO().foo();

// 		let co = MyCustomCO();
// 		print(co);

// 		// print(CompilerObject);

// 		print("[RT] Hello, World!");
// 		return;
// 	}
// }

// if (false) {
// 	print("[CT] Condition is true");
// } else {
// 	print("[CT] Condition is false");
// }

// // A.main();
