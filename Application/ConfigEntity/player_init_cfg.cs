using Application.Abstractions.Config;

namespace Application.ConfigEntity;

internal class player_init_cfg : IDataEntity
{
    public int id { get; init; }
    public int gold { get; init; }
    public string? items { get; init; }
}