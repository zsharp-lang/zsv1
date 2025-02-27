//import { list } from "std:list";
import { print, input } from "std:io";


module Program {
/*
	let todo: list[string] = [
		"Add list[T] type",
		"Add support for generic types in CO",
		"Add compiler for `for` statements",
		"Fix `for.currentItem` field so that using `let` doesn't force to initialize.",
		"Add compiler for `case` statements",
	];

	fun addTodo() {
		let item = input("Enter item to add: ");

		todo.add(item);
	}

	fun getTodos(): list[string] {
		var i = 1;

		for (let item = 0 in todo)
		{
			print(string(i) + ". " + item);
			i = i + 1;
		}
	}

	fun delTodo(): void {
		let index = i32.parse(input("Enter item number to remove: "));

		todo.removeAt(index - 1);
	}
	*/
	fun do(): bool {
		print("Welcome to The ToDo List App! Please choose what to do:");
		print("1. Add an item.");
		print("2. List all items.");
		print("3. Remove an item.");
		print("4. Exit program.");
		

		let cmd = input("> ");


		case (cmd) {
		when ("1") print("ADD"); // addTodo();
		when ("2") print("GET"); // getTodos();
		when ("3") print("DEL"); // delTodo();
		when ("4") return false;
		} else print("Unknown command: " + cmd);

		return true;
	}

	fun main(): void {
		while (do());	
		else print("Thank you for using The ToDo List App!");

		return;
	}
}
