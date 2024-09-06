using MessagePack;
using Newtonsoft.Json;
using SharedKernel.Protocols;
using SharedKernel;
using System.Data.SQLite;

namespace Test;

internal static class Tool
{
    public const int HeaderSize = 4;
    public const int ProtocolIdSize = 2;
    private static readonly Random _random = new Random(DateTime.UtcNow.Millisecond);

    private const string _onnectionString = "Data Source=G:/Workplace/SimpleServer/SqliteDB/Game.db;";

    private static readonly List<Guid> _playerIdList = new List<Guid>();

    static Tool()
    {
        using (SQLiteConnection connection = new SQLiteConnection(_onnectionString))
        {
            connection.Open();

            string query = "SELECT Id FROM Players ORDER BY Id";

            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Guid id = reader.GetGuid(0);
                        _playerIdList.Add(id);
                    }
                }
            }

            connection.Close();
        }
    }

    internal static Guid GetPlayerIdByIndex(int index)
    {
        if (index < _playerIdList.Count)
            return _playerIdList[index];

        return Guid.NewGuid();
    }

    internal static List<Guid> GetPlayerIdList()
    {
        return _playerIdList;
    }

    internal static byte[] PackMessage<T>(Guid playerId, short protocolId, T message)
    where T : BaseProtocol, IRequestProtocol, new()
    {
        byte[] messageBody = MessagePackSerializer.Serialize(message);

        byte[] protocolIdBytes = BitConverter.GetBytes(protocolId);

        byte[] playerIdBytes = playerId.ToByteArray();

        int lenth = protocolIdBytes.Length + messageBody.Length + playerIdBytes.Length;
        byte[] size = BitConverter.GetBytes(lenth);
        byte[] finalMessage = new byte[lenth + HeaderSize];
        Buffer.BlockCopy(size, 0, finalMessage, 0, HeaderSize);
        Buffer.BlockCopy(protocolIdBytes, 0, finalMessage, HeaderSize, protocolIdBytes.Length);
        Buffer.BlockCopy(messageBody, 0, finalMessage, HeaderSize + protocolIdBytes.Length, messageBody.Length);
        Buffer.BlockCopy(playerIdBytes, 0, finalMessage, HeaderSize + protocolIdBytes.Length + messageBody.Length, playerIdBytes.Length);

        return finalMessage;
    }

    internal static int Rand(int ceil)
    {
        return _random.Next(ceil);
    }

    internal static T? ReadJsonFromFile<T>(string filePath)
        where T : ClientInformation, new()
    {
        string json;
        if (!File.Exists(filePath)) return null;
        json = File.ReadAllText(filePath);
        var deserializedObject = JsonConvert.DeserializeObject<T>(json);

        return deserializedObject;
    }

    internal static async Task<(ProtocolId protocolId, INotificationProtocol protocol, Guid playerId)> UnpackMessage(byte[] packedMessage)
    {
        var protocolId = (ProtocolId)BitConverter.ToInt16(packedMessage, 0);

        var playerId = new Guid(packedMessage.Skip(packedMessage.Length - 16).ToArray());

        byte[] messageBody = packedMessage.Skip(ProtocolIdSize).Take(packedMessage.Length - 18).ToArray();

        var protocol = ProtocolProcessor.Instance.DeserializeMessage(protocolId, messageBody);
        return (protocolId, protocol, playerId);
    }

    internal static void WriteJsonToFile<T>(string filePath, T objectToWrite)
        where T : ClientInformation, new()
    {
        string json = JsonConvert.SerializeObject(objectToWrite, Formatting.Indented);

        File.WriteAllText(filePath, json);
    }
}