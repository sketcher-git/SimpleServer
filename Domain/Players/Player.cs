using SharedKernel;

namespace Domain.Players;

public sealed class Player : Entity
{
    private Player(Guid id, Email email, Name name, long loginTimestamp)
        : base(id)
    {
        Email = email;
        Name = name;
        LoginTimestamp = loginTimestamp;
    }

    private Player()
    {
    }

    public Email Email { get; private set; }

    public Name Name { get; private set; }

    public long LoginTimestamp { get; set; }

    public long LogOutTimestamp { get; set; }

    public static Player Create(Guid id, Email email, Name name, DateTime loginTime)
    {
        var player = new Player(id, email, name, loginTime.Ticks);

        return player;
    }
}