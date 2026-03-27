class Identity of ORMModel(tableName: false) {
    @primaryKey
    let id: UUID.4;
}

class User 
    : Identity
    of ORMModel
{
    @column(unique: true)
    let username: String[minLength=3, maxLength=29];
    @column
    let email: Regex["^[\\w.-]+@[\\w.-]+\\.\\w{2,}$"];
}

let q = User.where(
    User.username == "exampleUser"
);
let users = execute(q).all();

let user: Type[User] = User;
let info: ORMModel = reflect(User);

for (let column in info.columns) {
    case (column.type) {
    when |type| (_ is String) {
        if |min| (type.minLength) print("Column ${column.name} has min length ${min}");
        if |max| (type.maxLength) print("Column ${column.name} has max length ${max}");
    }
    when |type| (_ is Regex) {
        print("Column ${column.name} has regex pattern ${type.pattern}");
    }
    }
}

fun find(type: Type[Identity], id: UUID.4) {
    let q = type.where(
        type.id == id
    );
    return execute(q).first() as type?;
}
