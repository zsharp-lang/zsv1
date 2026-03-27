class UIContext {
    let user: User?;
}

let api = FastAPI("My Application API", prefix: "/api");
let ui = FastUI("My Application UI", context: UIContext);

api.add_middleware(
    StaticFiles(
        directory: "public",
        html: true,
        check_dir: true,
        prefix: "/static"
    )
);

class Config of ApplicationConfiguration {
    let host = "0.0.0.0";
    let ssl = false;
    let port = if (ssl) 443 else 80;
}

api.add_middleware(
    let apiDI = DI()
);
apiDI[User] = fun(token: Authentication) {
    import * from "pkg:jwt";

    let payload = decode(token);
    if (!payload) return HTTPError(401, "Invalid token");

    return User(
        id: payload.sub,
        name: payload.name
    );
};
apiDI[DBSession] = fun(config: Config) {
    return DBSession();
};

ui.add_middleware(
    let uiDI = DI()
);
uiDI[User] = fun(ctx: UIContext) {
    if (ctx.user) return ctx.user;

    return Redirect(Program.login);
}

module Program;

@api.get("/projects")
fun projects({
    user: User,
    db: DBSession
}) {
    let projects = db.query(
        inline SQL {
            SELECT 
                ${DBModels.Project.id}, 
                ${DBModels.Project.name}, 
                ${DBModels.Project.description} 
            FROM ${DBModels.Project}
            WHERE ${DBModels.Project.owner} = ${user}
        }
    );

    return projects.map(toJSON);
}

@ui.component()
fun listProjects({
    user: User,
}) {
    let projects = projects(user);
    return inline HTML {
        <div>
            <ul>
                {
                    for (project in projects) {
                        <li>{project.name}</li>
                    }
                }
            </ul>
        </div>
    };
}

@ui.page("/")
fun home({
    user: User,
}) {
    return inline HTML {
        <div>
            <h1>Welcome, {user.name}!</h1>
            <listProjects user={user} />
        </div>
    };
}

@ui.page("/login")
fun login({
    user: User?
}) {
    if (user) {
        return Redirect(home);
    }

    return inline HTML {
        <div>
            <h1>Login</h1>
            <form method="POST" action="/api/login">
                <input type="text" name="username" placeholder="Username" required />
                <input type="password" name="password" placeholder="Password" required />
                <button type="submit">Login</button>
            </form>
        </div>
    };
}

let config: Config;

fun main() {
    api.run(
        host: config.host,
        port: config.port
    );
}
