import { input, print } from "std:io";
import { Console, List, TestInterface, greet, id } from "net:ZLoad.Test.dll";
// import { Console } from "net:C:\\Program Files\\dotnet\\shared\\Microsoft.NETCore.App\\8.0.13\\System.Console.dll";

module Program;

class MyClass : TestInterface {
	new(this) {}
}

fun main(): void {
	let x = List[string]();

	let v = MyClass();
	
	print(greet(let name = input("Please enter your name: ")));

	Console.WriteLine(id[string]("Hi"));

	x.append(name);

	print("x[0] = " + x.get(0));

	return;
}
