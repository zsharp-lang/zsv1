import { input, print } from "std:io";


module Lib {
	fun id(x: type): type { return x; }

	fun str(s: string): string { return s; }

	fun printAndGetVoid(x: string): type { print("CT: " + x); return void; }
}


module Program;

var message: string;

fun main(): Lib.id(void) { 
	message = input("Please enter a message: "); 
	let audience = "World!";

	bar(audience); 

	{
		print("Hello");
	}

	var v = 10 - 3;

	print(string(v));

	v = 4;

	print(string(v));

	while (false)
	{
		print("TRUE");
		break;
	} else {
		print("FALSE");
	}

	return; 
}

fun bar(x: string): Lib.printAndGetVoid("This is executed at CT!") { 
	foo(message + ", " + Lib.str(x)); 
	return; 
}

fun foo(x: string): void { 
	print(x); 
	return; 
}


// import { compile } from "pkg:dotnet-compiler";

// let dotNETModule = compile(infoof(Program));
// dotNETModule.Write("Program.dll");
