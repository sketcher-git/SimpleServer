using Protocols;
using SharedKernel;
using System.Collections.Concurrent;

namespace Test;

internal class StressTesting
{
    private CancellationTokenSource _cts = new CancellationTokenSource();

    private const int _clientCount = 1024;
    private static readonly ConcurrentDictionary<int, ClientStress> _clientsMap = new ConcurrentDictionary<int, ClientStress>();

    private static readonly BlockingCollection<MessageObject> _clientMessageQueue = new BlockingCollection<MessageObject>();

    private List<string> chatContentList = new List<string>();

    internal StressTesting(string address, int port)
    {
        for (int i = 0; i < _clientCount; i++)
        {
            var client = new ClientStress(address, port, i);
            _clientsMap.TryAdd(i, client);
        }

        chatContentList.Add("Hello World!");
        chatContentList.Add("Is anybody here?");
        chatContentList.Add("Do not answer! Do not answer! Do not answer!");
        chatContentList.Add($"It's {DateTime.Today.DayOfWeek.ToString()} today.");
    }

    private async Task CreateClientAndTakeActions(CancellationToken token)
    {
        var clients = _clientsMap.Values.ToArray();
        int clientCount = clients.Length;
        for (int i = 0; i < clientCount; i++)
        {
            clients[i].ConnectAsync();
        }

        while (true)
        {
            try
            {
                for (int i = 0; i < clientCount; i++)
                {
                    var client = clients[i];
                    if (!client.IsOnline)
                    {
                        await Task.Delay(10, token);
                        continue;
                    }

                    int action = Tool.Rand(5);
                    int index = Tool.Rand(chatContentList.Count);
                    switch (action)
                    {
                        case 0:
                            int dataId = Tool.Rand(3) + 1;
                            client.BuyItem(dataId);
                            break;
                        case 1:
                            client.Chat(chatContentList[index]);
                            break;
                        case 2:
                            client.GetPlayerByIdQuery();
                            break;
                        default:
                            client.ConsumeItem();
                            break;
                    }

                    //To send 8 times per millisecond
                    if (i > 0 && (i & 7) == 0)
                        await Task.Delay(1, token);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }

    public static void Enqueue(MessageObject message, ProtocolId protocolId)
    {
        switch (protocolId)
        {
            case ProtocolId.Login:
            case ProtocolId.CreatePlayer:
            case ProtocolId.ItemList:
            case ProtocolId.BuyItem:
            case ProtocolId.ConsumeItem:
                _clientMessageQueue.Add(message);
                break;
            default:
                break;
        }
    }

    private async Task StartProcess(CancellationToken token)
    {
        while (true)
        {
            var message = _clientMessageQueue.Take();
            if (message == null)
            {
                await Task.Delay(100, token);
                continue;
            }

            var response = Tool.UnpackMessage(message.Message);
            var protocol = response.protocol;
            if (protocol == null)
                continue;

            _clientsMap.TryGetValue(message.PlayerIndex, out var client);
            switch (response.protocolId)
            {
                case ProtocolId.Login:
                    client.LoginDone(protocol as LoginResponseProtocol);
                    break;
                case ProtocolId.ItemList:
                    client.ItemListQueryDone(protocol as ItemListResponseProtocol);
                    break;
                case ProtocolId.CreatePlayer:
                    client.CreateDone(protocol as PlayerCreateResponseProtocol);
                    break;
                case ProtocolId.BuyItem:
                    client.BuyItemDone(protocol as BuyItemResposeProtocol);
                    break;
                case ProtocolId.ConsumeItem:
                    client.ConsumeItemDone(protocol as ConsumeItemResponseProtocol);
                    break;
                default:
                    break;
            }
        }
    }

    internal void StartStressTesting()
    {
        Task.Factory.StartNew(() => CreateClientAndTakeActions(_cts.Token));
        Task.Factory.StartNew(() => StartProcess(_cts.Token));
        Console.WriteLine("StressTesting thread started!");
    }

    internal void StopStressTesting()
    {
        _cts.Cancel();
    }
}