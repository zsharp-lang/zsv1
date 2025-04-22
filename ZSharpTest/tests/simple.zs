import { List, input, print } from "std:io";
import { Console, greet, id } from "net:ZLoad.Test.dll";
// import { Console } from "net:C:\\Program Files\\dotnet\\shared\\Microsoft.NETCore.App\\8.0.13\\System.Console.dll";

module Program;

fun main(): void {
	let x = List[string]();
	
	print(greet(let name = input("Please enter your name: ")));

	Console.WriteLine(id[string]("Hi"));

	x.append(name);

	print("x[0] = " + x.get(0));

	return;
}
