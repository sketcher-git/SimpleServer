namespace Test;

internal class StartClient(string address, int port)
{
    internal void StartSingleClient()
    {
        Console.WriteLine($"TCP server address: {address}");
        Console.WriteLine($"TCP server port: {port}");

        Console.WriteLine();

        var client = new Client(address, port);

        Console.Write("Client connecting...");
        client.ConnectAsync();
        Console.WriteLine("Done!");

        for (; ; )
        {
            string line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
                break;

            if (line.StartsWith("buy"))
            {
                string[] param = line.Split(" ");
                client.BuyItem(Convert.ToInt32(param[1]), param.Length > 2 ? Convert.ToInt32(param[2]) : 1);
            }

            if (line.StartsWith("use"))
            {
                string[] param = line.Split(" ");
                client.ConsumeItem(param.Length > 1 ? Convert.ToInt32(param[1]) : 1);
            }

            if (line.StartsWith("chat"))
            {
                string[] param = line.Split(":");
                client.Chat(param.Length > 1 ? param[1] : "hello");
            }

            if (line == "1")
            {
                client.GetPlayerByIdQuery();
            }
        }

        Console.Write("Client disconnecting...");
        client.DisconnectAndStop();
        Console.WriteLine("Done!");
    }

    internal void StartStressTesting()
    {
        var stress = new StressTesting(address, port);
        Console.Write("Start stress testing...");
        stress.StartStressTesting();
        Console.WriteLine("Done!");

        for (; ; )
        {
            string line = Console.ReadLine();
            if (string.IsNullOrEmpty(line))
                break;
        }

        Console.Write("Stop stress testing...");
        stress.StopStressTesting();
        Console.WriteLine("Done!");
    }
}
