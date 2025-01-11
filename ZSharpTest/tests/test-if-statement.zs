import { print } from "std:io";

module Program;


fun main(): void {
	let value = false;

	if (value)
		print("true");
	else if (false)
		print("false");
	else
		print("else");

	return;
}
