// The server an endpoint belongs to, which `register` gives, so an endpoint can send what
// it wasn't asked for, as a notification.

let current: Server = null;

// Makes a server the one endpoints send through.
fun use(server: Server) { current = server; }

// Sends a notification to the client, with its parameters as JSON.
fun publish(method: String, parameters: JsonValue) {
    current.notify(method, parameters);
}
