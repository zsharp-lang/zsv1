/* Required Features:
 * [-] Primary constructor
 * [-] Auto-fields from primary constructor parameters
 * [x] Fields
 * [x] Anonymous constructor
 * [x] Named constructor
 * [x] Methods
 * [x] Case-Of
 * [ ] Automatic type inference for first parameter in methods
 * [ ] Automatic instance binding when accessing instance method/field from instance method
 * [ ] Instance method as function with `this` first parameter
 * [x] Module globals initializers
 */


module Program;


class Authentication {
	var username: string;
	var password: string;

	new(this, username: string, password: string) {
		this.username = username;
		this.password = password;
	}
}


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

	fun login(auth: Authentication, this: User): bool {
		return this.login(auth.username, auth.password);
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

	case (Authentication(username, password)) of (User.login) {
	when (user) print("USER"); // userTerminal();
	when (admin) print("ADMIN"); // adminTerminal();
	} else print("Invalid credentials.");
}


fun main(): void {
	while (mainMenu()) ;
	else print("Thank you for using Simple User System!");

	return;
}


let user = User("user", "123");
let admin = User.Admin("admin", "321");
