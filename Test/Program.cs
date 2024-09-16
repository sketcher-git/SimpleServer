using Test;

Console.WriteLine("Hello, World!");

// TCP server address
string address = "127.0.0.1";
if (args.Length > 0)
    address = args[0];

// TCP server port
int port = 3069;
if (args.Length > 1)
    port = int.Parse(args[1]);

var start = new StartClient(address, port);
//start.StartSingleClient();
start.StartStressTesting();