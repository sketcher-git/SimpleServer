using Protocols;
using SharedKernel;
using SharedKernel.Protocols;
using System.Net.Sockets;
using TcpClient = NetCoreServer.TcpClient;

namespace Test;

internal class Client : TcpClient
{
    private bool _stop;
    private Guid _playerId = Guid.Empty;
    private string _jsonPath = "ClientInformation.json";
    private long _loginTimestamp;
    private long _serverTimestamp;

    private readonly Dictionary<Guid, (Guid itemId, int itemDataId, int itemAmount)> _itemMap = new Dictionary<Guid, (Guid itemId, int itemDataId, int itemAmount)>();

    public Client(string address, int port) : base(address, port) { }

    public void DisconnectAndStop()
    {
        _stop = true;
        DisconnectAsync();
        while (IsConnected)
            Thread.Yield();
    }

    protected override void OnConnected()
    {
        Console.WriteLine($"Test TCP client connected a new session with Id {Id}");
        Login();
    }

    protected override void OnDisconnected()
    {
        Console.WriteLine($"Test TCP client disconnected a session with Id {Id}");

        // Wait for a while...
        Thread.Sleep(1000);

        // Try to connect again
        if (!_stop)
            ConnectAsync();
    }

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"Test TCP client caught an error with code {error}");
    }

    protected override async void OnReceived(byte[] buffer, long offset, long size)
    {
        int processedBytes = 0;
        while (processedBytes < size)
        {
            if (size - processedBytes < Tool.HeaderSize)
            {
                throw new Exception("receive zero head");
            }

            int messageLength = BitConverter.ToInt32(buffer, (int)offset + processedBytes);
            if (size - processedBytes - Tool.HeaderSize < messageLength)
                break;

            byte[] packedMessage = new byte[messageLength];
            Array.Copy(buffer, (int)offset + Tool.HeaderSize, packedMessage, 0, messageLength);
            var response = Tool.UnpackMessage(packedMessage);
            var protocol = response.protocol;
            switch (response.protocolId)
            {
                case ProtocolId.Login:
                    LoginDone(protocol as LoginResponseProtocol);
                    break;
                case ProtocolId.CreatePlayer:
                    CreateDone(protocol as PlayerCreateResponseProtocol);
                    break;
                case ProtocolId.Chat:
                    ChatDone(protocol as ChatResponseProtocol);
                    break;
                case ProtocolId.ChatNotification:
                    ChatNotification(protocol as ChatNotificationProtocol);
                    break;
                case ProtocolId.LoginNotification:
                    LoginNotification(protocol as LoginNotificationProtocol);
                    break;
                case ProtocolId.HeartbeatNotification:
                    var p = protocol as HeartbeatNotificationProtocol;
                    _serverTimestamp = p.NowTimestamp;
                    //Console.WriteLine($"Game server timestamp is {p.NowTimestamp}");
                    break;
                case ProtocolId.ItemList:
                    ItemListQueryDone(protocol as ItemListResponseProtocol);
                    break;
                case ProtocolId.BuyItem:
                    BuyItemDone(protocol as BuyItemResposeProtocol);
                    break;
                case ProtocolId.ConsumeItem:
                    ConsumeItemDone(protocol as ConsumeItemResponseProtocol);
                    break;
                case ProtocolId.PlayerInfo:
                    GetPlayerByIdQueryDone(protocol as PlayerInformationResponseProtocol);
                    break;
                default:
                    break;
            }

            processedBytes += Tool.HeaderSize + messageLength;
        }
    }

    public void BuyItem(int dataId, int amount = 1)
    {
        var query = new BuyItemRequestProtocol
        {
            DataId = dataId,
            Amount = amount
        };
        Send((short)query.ProtocolId, query);
    }

    private void BuyItemDone(BuyItemResposeProtocol response)
    {
        var item = response.Item;
        Guid itemId = item.itemId;
        int dataId = item.itemDataId;
        int amount = item.itemAmount;
        switch (response.ErrorType)
        {
            case ErrorType.None:
                Console.WriteLine($"Item with Id = '{itemId}', data id = '{dataId}', item count = '{amount}'");
                _itemMap[itemId] = item;
                break;
            case ErrorType.NotFound:
                Console.WriteLine($"The item with the Data Id = '{dataId}' was non-existent");
                break;
            case ErrorType.Validation:
                Console.WriteLine($"The item records was unloaded");
                break;
            case ErrorType.Failure:
                Console.WriteLine($"The player with the Id = '{_playerId}' attampted to buy {amount} items");
                break;
            case ErrorType.Conflict:
            default:
                break;
        }
    }

    public void Chat(string contont)
    {
        ChatRequestProtocol query = new ChatRequestProtocol
        {
            Channel = ChatType.World,
            TargetId = Guid.Empty,
            Content = contont
        };
        Send((short)query.ProtocolId, query);
    }

    private void ChatDone(ChatResponseProtocol response)
    {
        switch (response.ErrorType)
        {
            case ErrorType.None:
                Console.WriteLine("Chat is done");
                break;
            case ErrorType.NotFound:
                Console.WriteLine($"Target player with the Id = '{response.TargetId}' was offline");
                break;
            case ErrorType.Failure:
            case ErrorType.Validation:
            case ErrorType.Conflict:
            default:
                break;
        }
    }

    private void ChatNotification(ChatNotificationProtocol notification)
    {
        if (notification.SenderId == _playerId)
            return;

        Console.WriteLine($"Player with Id = '{notification.SenderId}' sent '{notification.Content}' on channel '{notification.Channel.ToString()}' at '{notification.SendingTime}'");
    }

    public void ConsumeItem(int amount = 1)
    {
        var query = new ConsumeItemRequestProtocol
        {
            ItemId = _itemMap.First().Key,
            Amount = amount
        };
        Send((short)query.ProtocolId, query);
    }

    private void ConsumeItemDone(ConsumeItemResponseProtocol response)
    {
        var item = response.Item;
        Guid itemId = item.itemId;
        switch (response.ErrorType)
        {
            case ErrorType.None:
                int amount = item.itemAmount;
                Console.WriteLine($"Item with Id = '{itemId}', data id = '{item.itemDataId}', item count = '{amount}'");
                if (amount > 0)
                    _itemMap[itemId] = item;
                else
                    _itemMap.Remove(itemId);
                break;
            case ErrorType.NotFound:
                Console.WriteLine($"The item with the Data Id = '{itemId}' was non-existent");
                _itemMap.Remove(itemId);
                break;
            case ErrorType.Validation:
                Console.WriteLine($"The item records was unloaded");
                break;
            case ErrorType.Failure:
                Console.WriteLine($"The item with the Id = '{itemId}' was insufficient");
                break;
            case ErrorType.Conflict:
            default:
                break;
        }
    }

    private void Create()
    {
        int remain = DateTime.UtcNow.Millisecond % 100;
        PlayerCreateRequestProtocol playerCreate = new PlayerCreateRequestProtocol
        {
            Email = $"test{remain}@test1.com",
            Name = $"testName{remain}"
        };
        Send((short)playerCreate.ProtocolId, playerCreate);
    }

    private void CreateDone(PlayerCreateResponseProtocol response)
    {
        switch (response.ErrorType)
        {
            case ErrorType.None:
                Console.WriteLine($"Player with Id = '{_playerId}' created");
                ItemListQuery();
                break;
            case ErrorType.Conflict:
                Console.WriteLine($"Email or Name is not unique");
                break;
            case ErrorType.Validation:
                Console.WriteLine($"Email or Name is invalid");
                break;
            case ErrorType.Failure:
                Console.WriteLine($"The player with the Id = '{_playerId}' already exists");
                break;
            case ErrorType.NotFound:
            default:
                break;
        }
    }

    public void GetPlayerByIdQuery()
    {
        var query = new PlayerInformationRequestProtocol
        {
            TargetPlayerId = Guid.Parse("97B010C6-D15A-41D5-B834-6308D2EDA4F8")
        };
        Send((short)query.ProtocolId, query);
    }

    private void GetPlayerByIdQueryDone(PlayerInformationResponseProtocol response)
    {
        Guid playerId = response.PlayerInfo.playerId;
        switch (response.ErrorType)
        {
            case ErrorType.None:
                Console.WriteLine($"Player {playerId} name is {response.PlayerInfo.playerName}, login time is {new DateTime(response.PlayerInfo.loginTimestamp)}");
                break;
            case ErrorType.NotFound:
                Console.WriteLine($"The player with the Id = '{playerId}' was not found");
                break;
            case ErrorType.Conflict:
            case ErrorType.Failure:
            case ErrorType.Validation:
            default:
                break;
        }
    }

    private void ItemListQuery()
    {
        var query = new ItemListRequestProtocol();

        Send((short)query.ProtocolId, query);
    }

    private void ItemListQueryDone(ItemListResponseProtocol response)
    {
        switch (response.ErrorType)
        {
            case ErrorType.None:
                Console.WriteLine($"Items of Player with Id = '{_playerId}' were loaded");
                var itemMap = response.ItemMap;
                if (itemMap == null) break;
                foreach (var item in itemMap.Values)
                {
                    Console.WriteLine($"ItemId = '{item.itemId}', itemDataId = '{item.itemDataId}', itemAmount = '{item.itemAmount}'");
                    _itemMap.Add(item.itemId, item);
                }
                break;
            case ErrorType.Validation:
                Console.WriteLine($"The query about <item records of player with the Id = '{_playerId}'> was invalid. The player had to login to the server first");
                break;
            case ErrorType.Conflict:
            case ErrorType.Failure:
            case ErrorType.NotFound:
            default:
                break;
        }
    }

    private void Login()
    {
        var info = Tool.ReadJsonFromFile<ClientInformation>(_jsonPath);
        if (info == null)
        {
            _playerId = Guid.NewGuid();
            Tool.WriteJsonToFile(_jsonPath, new ClientInformation
            {
                PlayerId = _playerId,
                Email = "",
                Name = ""
            });
        }
        else
            _playerId = info.PlayerId;

        LoginRequestProtocol login = new LoginRequestProtocol();
        Send((short)login.ProtocolId, login);
    }

    private void LoginDone(LoginResponseProtocol response)
    {
        switch (response.ErrorType)
        {
            case ErrorType.None:
                _loginTimestamp = response.LoginTimestamp;
                Console.WriteLine($"Player with Id '{_playerId}' logined and the timestamp is '{_loginTimestamp}'");
                ItemListQuery();
                break;
            case ErrorType.NotFound:
                Console.WriteLine($"Player with the Id = '{_playerId}' was not found");
                Create();
                break;
            case ErrorType.Failure:
            case ErrorType.Validation:
            case ErrorType.Conflict:
            default:
                break;
        }
    }

    private void LoginNotification(LoginNotificationProtocol notification)
    {
        if (notification.PlayerId == _playerId)
            return;

        Console.WriteLine($"Player '{notification.Name}' with the Id = '{notification.PlayerId}' was online");
    }

    private void Send<T>(short protocolId, T request)
        where T : BaseProtocol, IRequestProtocol, new()
    {
        byte[] buffer = Tool.PackMessage(_playerId, protocolId, request);
        SendAsync(buffer);
    }
}