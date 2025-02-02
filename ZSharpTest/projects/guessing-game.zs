import { input, print } from "std:io";
import { ceil, log2, random } from "std:math";

module Program;

fun guessOnce(guess: i32, number: i32, tries: i32): bool {
	if (guess < number)
		print("Too low!");
	else if (number < guess)
		print("Too high!");
	else return true;

	return false;
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

		if (guessOnce(guess, number, tries)) 
		{
			print("You got it in " + string(tries + 1) + " tries!");
			break;
		}
		else tries = tries + 1;
	} else print("You didn't get it in " + string(maxTries) + " tries. The number was " + string(number));

	return;
}
