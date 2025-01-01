import { input, print } from "std:io";
import { ceil, log2, random, increment } from "std:math";
//import { random } from "std:random";

module Program;

fun guessOnce(guess: i32, number: i32, tries: i32): bool {
	while (guess < number)
	{
		print("Too low!");
		return false;
	}
	while (number < guess)
	{
		print("Too high!");
		return false;
	}

	print("You got it in " + string(increment(tries)) + " tries!");
	return true;
}

fun main(): void {
	let minNumber = 1;
	let maxNumber = 100;

	let number = random(minNumber, maxNumber);
	let maxTries = ceil(log2(maxNumber - minNumber));

	var tries = 0;

	while (tries < maxTries) {
		let guessString = input("Guess a number between " + string(minNumber) + " and " + string(maxNumber) + ": ");
		let guess = i32.parse(guessString);

		while (guessOnce(guess, number, tries))
		{
			tries = increment(maxTries);
			break;
		} else tries = increment(tries);
	}

	while (maxTries < tries) { break; }
	else print("You didn't get it in " + string(maxTries) + " tries. The number was " + string(number));

	return;
}
