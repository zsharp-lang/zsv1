// Imports
import {
    FastAPI
} from "pkg:fastapi";
import {
    FastUI
} from "pkg:fastui";

// Declare stack context
let api = FastAPI(
    prefix: "/api",
    context: class {

    }
);
{
    api.addMiddleware(
        let di = DependencyInjector(
            context: api.context
        );
    );
    di[User];
    di[UserRepository];
}

let ui = FastUI(
    context: class {
        user: User?

        fun isSignedIn(): Bool {
            return user;
        }
    }
);
{
    ui.addMiddleware(
        let di = DependencyInjector(
            context: ui.context
        );
    );
    di[User];
}


// Requests module

module Requests {
    class Login of Request {
        username: String;
        password: String;
    }
}

// API module

module API {
    import {
        formData
    } from "pkg:fastapi";

    @api.post("/login")
    fun login({
        @formData credentials: Requests.Login,
        users: UserRepository
    }) {
        let user = users.findByUsername(credentials.username);
        if (!user || user.password != credentials.password) {
            return HTTPError.Unauthorized("Invalid credentials");
        }

        import { encode } from "pkg:jwt";
        let token = encode(
            sub: user.id,
            name: user.name
        );

        return {
            token: token
        };
    }

    @api.get("/me")
    fun me({
        user: User
    }) {
        return user;
    }
}


// UIComponents module

module UIComponents {
    import "pkg:html";

    @ui.component()
    fun userInfo({
        user: User
    }) {
        return inline HTML {
            <div>
                <h2>{user.name}</h2>
                <p>ID: {user.id}</p>
            </div>
        };
    }
}


// UI module

module UI {
    import {
        Redirect
    } from "pkg:fastui";

    @ui.page("/")
    fun home({
        user: User?
    }) {
        if (!user) {
            return Redirect(login());
        }

        return inline HTML {
            <div>
                <h1>Welcome, {user.name}!</h1>
            </div>
        };
    }

    @ui.page("/me")
    fun me({
        user: User
    }) {
        return inline HTML {
            <div>
                <h1>My Profile</h1>
                <userInfo user={user} />
            </div>
        };
    }

    @ui.page("/login")
    fun login({
         user: User?
     }) {
         if (user) {
             return Redirect(home());
         }

         return inline HTML {
             <div>
                 <h1>Login</h1>
                 <form action=${API.login}>
                     <input type="text" field=${Requests.Login.username} placeholder="Username" required />
                     <input type="password" field=${Requests.Login.password} placeholder="Password" required />
                     <button type="submit">Login</button>
                 </form>
             </div>
         };
     }
}


// Environments

class ProductionEnvironment of ExecutionEnvironment[api.context] {
    import {
        Header,
        HTTPError
    } from "pkg:fastapi";
    import {
        Headers,
        AuthorizationHeaderType
    } from "pkg:fastapi/standard";

    @provider()
    fun(token: Header[Headers.Authorization, Token: AuthorizationHeaderType.Bearer]): User | HTTPError {
        import { decode } from "pkg:jwt";

        let payload = decode[UserToken](token);
        if (!payload) return HTTPError.Unauthorized("Invalid token");

        return User(
            id: payload.sub,
            name: payload.name
        );
    }
}

class DevelopmentEnvironment of ExecutionEnvironment[api.context] {
    @provider()
    fun() {
        return User(
            id: "123",
            name: "Test User"
        );
    }
}


class UIProvider if ExecutionEnvironment[ui.context] {
    import {
        Redirect
    } from "pkg:fastui";

    @provider()
    fun(ctx: ui.context) {
        if (!ctx.user) {
            return Redirect(UI.login);
        }

        return ctx.user;
    }
}


// Build

import { config } from "core:compiler/project";
api.middleware[DependencyInjector].use(
    case (config.environment) {
        "production" => ProductionEnvironment,
        "development" => DevelopmentEnvironment,
        _ => throw new Error("Unknown environment: ${config.environment}")
    }
);
ui.middleware[DependencyInjector].use(UIProvider);

import { FullStackBuilder } from "pkg:fastapp";

let builder = FullStackBuilder(
    [
        Backend(API, dir: "backend"),
        Frontend(UIComponents, UI, dir: "frontend")
    ]
);
builder.build();