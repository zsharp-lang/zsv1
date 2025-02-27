﻿import { input, print } from "std:io";
import { ceil, log2, random } from "std:math";

module Program;

let minNumber = 1;
let maxNumber = 100;

let maxTries = ceil(log2(maxNumber - minNumber));
let number = random(minNumber, maxNumber);

let inputMessage = "Guess a number between " + string(minNumber) + " and " + string(maxNumber) + ": ";

fun guessOnce(guess: i32, number: i32, tries: i32): bool {
	case (true) {
	when (guess < number) print("Too low!");
	when (number < guess) print("Too high!");
	} else return true;

	return false;
}

fun main(): void {
	var tries = 0;

	while (tries < maxTries) {
		let guessString = input(inputMessage);
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
