namespace SharedKernel;

public enum ChatType
{
    World = 1,
    Local,
    Private
}

public enum LogLevelType
{
    Notice = 1,
    Warning,
    Error,
}

public enum ProtocolId
{
    Login = 1,
    Logout = 2,
    CreatePlayer = 3,
    Chat = 4,
    ChatNotification,
    LoginNotification,
    HeartbeatNotification,
    ItemList,
    BuyItem,
    ConsumeItem,
    PlayerInfo
}

public enum ResponseType
{
    Common,
    Broadcast = 1,
}