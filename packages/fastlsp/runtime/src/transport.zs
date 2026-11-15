// How messages get to a server and back: a transport reads the text of one message at a
// time, and writes one. `Stdio` is the protocol's usual one, with its headers, over the
// process's standard streams; `MemoryTransport` is a list of messages, for a test.

def Transport of Interface {
    // The text of the next message, or null when there are no more.
    fun read(): String;

    // Sends the text of a message.
    fun write(text: String);
}

// A message in a list, and the messages written back, with no process in between.
def MemoryTransport : Transport {
    let incoming: List[String] = List[String]();
    let sent: List[String] = List[String]();
    let position: Int32 = 0;

    new() { }

    fun send(text: String) { incoming.Add(text); }

    fun read(): String {
        if (position >= incoming.Count) return null;
        position = position + 1;
        return incoming[position - 1];
    }

    fun write(text: String) { sent.Add(text); }
}

// Messages over the standard streams, each with the header of the base protocol:
// `Content-Length: <bytes>`, an empty line, and the content in UTF-8.
def Stdio : Transport {
    let input: Stream = Console.OpenStandardInput();
    let output: Stream = Console.OpenStandardOutput();

    new() { }

    // A line of the header, without its line ending, or null at the end of the input.
    fun readLine(): String {
        let line = StringBuilder();
        let next = input.ReadByte();
        if (next < 0) return null;
        while (next != 10) {
            if (next != 13) line.Append(Convert.ToChar(next).ToString());
            next = input.ReadByte();
            if (next < 0) return line.ToString();
        }
        return line.ToString();
    }

    fun read(): String {
        let length = -1;
        let line = readLine();
        while (line != null) {
            if (line == "") {
                if (length >= 0) return content(length);
            } else if (line.StartsWith("Content-Length:")) {
                length = Int32.Parse(line.Substring(15).Trim());
            }
            line = readLine();
        }
        return null;
    }

    fun content(length: Int32): String {
        let buffer = Byte[length];
        let read = 0;
        while (read < length) {
            let count = input.Read(buffer, read, length - read);
            if (count <= 0) return null;
            read = read + count;
        }
        return Encoding.UTF8.GetString(buffer);
    }

    fun write(text: String) {
        let body = Encoding.UTF8.GetBytes(text);
        let header = Encoding.ASCII.GetBytes("Content-Length: " + body.Length + "\r\n\r\n");
        output.Write(header, 0, header.Length);
        output.Write(body, 0, body.Length);
        output.Flush();
    }
}
