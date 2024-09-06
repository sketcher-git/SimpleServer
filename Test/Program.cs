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

Console.WriteLine($"TCP server address: {address}");
Console.WriteLine($"TCP server port: {port}");

//Console.WriteLine();

//// Create a new TCP chat client
//var client = new Client(address, port);

//// Connect the client
//Console.Write("Client connecting...");
//client.ConnectAsync();
//Console.WriteLine("Done!");

//Console.WriteLine("Press Enter to stop the client or '!' to reconnect the client...");

var stress = new StressTesting(address, port);
stress.StartStressTesting();

// Perform text input
for (; ; )
{
    string line = Console.ReadLine();
    if (string.IsNullOrEmpty(line))
        break;

    //// Disconnect the client
    //if (line == "!")
    //{
    //    Console.Write("Client disconnecting...");
    //    client.DisconnectAsync();
    //    Console.WriteLine("Done!");
    //    continue;
    //}

    //if (line.StartsWith("buy"))
    //{
    //    string[] param = line.Split(" ");
    //    client.BuyItem(Convert.ToInt32(param[1]), param.Length > 2 ? Convert.ToInt32(param[2]) : 1);
    //}

    //if (line.StartsWith("use"))
    //{
    //    string[] param = line.Split(" ");
    //    client.ConsumeItem(param.Length > 1 ? Convert.ToInt32(param[1]) : 1);
    //}

    //if (line.StartsWith("chat"))
    //{
    //    string[] param = line.Split(":");
    //    client.Chat(param.Length > 1 ? param[1] : "hello");
    //}

    //if (line == "1")
    //{
    //    client.GetPlayerByIdQuery();
    //}
}

// Disconnect the client
Console.Write("Client disconnecting...");
//client.DisconnectAndStop();
stress.StopStressTesting();
Console.WriteLine("Done!");