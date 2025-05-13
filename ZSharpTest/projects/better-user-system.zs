/* Required Features:
 * [-] Primary constructor
 * [-] Auto-fields from primary constructor parameters
 * [x] Fields
 * [x] Anonymous constructor
 * [x] Named constructor
 * [x] Methods
 * [x] Case-Of
 * [x] Automatic type inference for first parameter in methods
 * [x] Automatic instance binding when accessing instance method/field from instance method
 * [x] Instance method as function with `this` first parameter
 * [x] Module globals initializers
 */


import { input, print } from "std:io";
import { List } from "net:ZLoad.Test.dll";


module Program;


class User {
	var isAdmin: bool;

	var username: string;
	var password: string;

	new(this, username: string, password: string) {
		isAdmin = false;

		this.username = username;
		this.password = password;
	}

	new Admin(this, username: string, password: string) {
		User(this: this, username, password);

		isAdmin = true;
	}

	fun login(this, username: string, password: string): bool {
		if (username != this.username) return false;
		if (password != this.password) return false;

		return true;
	}
}


fun mainMenu(): bool {
	print("|== Main Menu ==|");
	print("1. Sign In");
	print("2. Exit Program");

	let userInput = input("> ");

	case (userInput) {
	when ("1") signIn();
	when ("2") return false;
	} else print("Invalid input.");

	return true;
}


fun signIn(): void {
	let username = input("Username: ");
	let password = input("Password: ");


	for (let user in users) {
		if (user.login(username, password)) {
			print("Successfully logged in to user: " + user.username);
			break;
		}
	} else {
		print("Could not log in with provided credentials");
	}

	return;
}


fun main(): void {
	while (mainMenu()) ;
	else print("Thank you for using Better User System!");

	return;
}


let users: List[User] = [
	User("user", "123"),
	User.Admin("admin", "321")
];
