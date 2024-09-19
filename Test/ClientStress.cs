using Protocols;
using SharedKernel;
using SharedKernel.Protocols;
using System.Collections.Concurrent;
using System.Net.Sockets;
using TcpClient = NetCoreServer.TcpClient;

namespace Test;

internal class ClientStress : TcpClient
{
    private bool _stop;
    private Guid _playerId = Guid.Empty;
    private int _index = 0;

    private ConcurrentDictionary<Guid, (Guid itemId, int itemDataId, int itemAmount)> _itemMap;

    public ClientStress(string address, int port, int index) : base(address, port)
    { 
        _index = index;
        _itemMap = new ConcurrentDictionary<Guid, (Guid, int, int)>();
    }

    public Guid PlayerId { get => _playerId; }

    public bool IsOnline { get; private set; }

    public void DisconnectAndStop()
    {
        _stop = true;
        DisconnectAsync();
        while (IsConnected)
            Thread.Yield();
    }

    protected override void OnConnected()
    {
        //Console.WriteLine($"Test TCP client connected a new session with Id {Id}");
        _playerId = Tool.GetPlayerIdByIndex(_index);
        Login();
    }

    protected override void OnDisconnected()
    {
        Console.WriteLine($"Test TCP client disconnected a session with Id {Id}");
    }

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"Test TCP client caught an error with code {error}");
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        int processedBytes = 0;
        while (processedBytes < size)
        {
            if (size - processedBytes < Tool.HeaderSize)
                throw new Exception("receive zero head");

            int messageLength = BitConverter.ToInt32(buffer, (int)offset + processedBytes);
            if (size - processedBytes - Tool.HeaderSize < messageLength)
                break;

            byte[] packedMessage = new byte[messageLength];
            Array.Copy(buffer, (int)offset + Tool.HeaderSize, packedMessage, 0, messageLength);
            var messageObj = new MessageObject
            {
                PlayerIndex = _index,
                Message = packedMessage
            };

            var protocolId = (ProtocolId)BitConverter.ToInt16(packedMessage, 0);
            StressTesting.Enqueue(messageObj, protocolId);

            processedBytes += (Tool.HeaderSize + messageLength);
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

    public void BuyItemDone(BuyItemResposeProtocol response)
    {
        var item = response.Item;
        switch (response.ErrorType)
        {
            case ErrorType.None:
                _itemMap[item.itemId] = item;
                break;
            case ErrorType.NotFound:
                Console.WriteLine($"The item with the Data Id = '{item.itemDataId}' was non-existent");
                break;
            case ErrorType.Validation:
                Console.WriteLine($"The item records was unloaded");
                break;
            case ErrorType.Failure:
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

    public void ConsumeItem(int amount = 1)
    {
        var itemId = Guid.Empty;
        if (_itemMap.Count > 0)
        {
            itemId = _itemMap.First().Key;
        }

        if (itemId == Guid.Empty)
        {
            return;
        }

        var query = new ConsumeItemRequestProtocol
        {
            ItemId = itemId,
            Amount = amount
        };
        Send((short)query.ProtocolId, query);
    }

    public void ConsumeItemDone(ConsumeItemResponseProtocol response)
    {
        var item = response.Item;
        Guid itemId = item.itemId;
        switch (response.ErrorType)
        {
            case ErrorType.None:
                if(item.itemAmount > 0)
                    _itemMap[itemId] = response.Item;
                else
                    _itemMap.TryRemove(itemId, out _);
                break;
            case ErrorType.NotFound:
                Console.WriteLine($"The item with the Id = '{itemId}' was non-existent");
                _itemMap.TryRemove(itemId, out _);
                break;
            case ErrorType.Validation:
            case ErrorType.Failure:
            case ErrorType.Conflict:
            default:
                break;
        }
    }

    private void Create(string? email = "")
    {
        int index = Tool.Rand(10000);
        int offset = Tool.Rand(10000);
        PlayerCreateRequestProtocol playerCreate = new PlayerCreateRequestProtocol
        {
            Email = string.IsNullOrEmpty(email) ? $"test{index}@test{offset}.com" : email,
            Name = $"testName{index}"
        };
        Send((short)playerCreate.ProtocolId, playerCreate);
    }

    internal void CreateDone(PlayerCreateResponseProtocol response)
    {
        switch (response.ErrorType)
        {
            case ErrorType.None:
                Console.WriteLine($"Player with Id = '{_playerId}' created");
                ItemListQuery();
                break;
            case ErrorType.Conflict:
                Console.WriteLine($"Email or Name is not unique");
                Create(response.Email);
                break;
            case ErrorType.Validation:
                Console.WriteLine($"Email or Name is invalid");
                Create(response.Email);
                break;
            case ErrorType.Failure:
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

    private void ItemListQuery()
    {
        var query = new ItemListRequestProtocol();

        Send((short)query.ProtocolId, query);
    }

    internal void ItemListQueryDone(ItemListResponseProtocol response)
    {
        switch (response.ErrorType)
        {
            case ErrorType.None:
                var itemMap = response.ItemMap;
                if (itemMap is not null)
                {
                    foreach (var item in itemMap)
                    {
                        _itemMap.TryAdd(item.Key, item.Value);
                    }
                }
                IsOnline = true;
                break;
            case ErrorType.Validation:
                Console.WriteLine($"The query about <item records of player with the Id = '{_playerId}'> was invalid. The player had to login to the server first");
                break;
            case ErrorType.Failure:
            case ErrorType.Conflict:
            case ErrorType.NotFound:
            default:
                break;
        }
    }

    private void Login()
    {
        Console.WriteLine($"Player with Id = '{_playerId}' and index = '{_index}' start to login");
        LoginRequestProtocol login = new LoginRequestProtocol();
        Send((short)login.ProtocolId, login);
    }

    internal void LoginDone(LoginResponseProtocol response)
    {
        switch (response.ErrorType)
        {
            case ErrorType.None:
                ItemListQuery();
                break;
            case ErrorType.NotFound:
                Create();
                break;
            case ErrorType.Failure:
            case ErrorType.Validation:
            case ErrorType.Conflict:
            default:
                break;
        }
    }

    private void Send<T>(short protocolId, T request)
        where T : BaseProtocol, IRequestProtocol, new()
    {
        byte[] buffer = Tool.PackMessage(_playerId, protocolId, request);
        SendAsync(buffer);
    }
}