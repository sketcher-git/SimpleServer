using Application.Abstractions.Config;

namespace Application.ConfigEntity;

internal class player_init_cfg : IDataEntity
{
    public int id { get; set; }
    public int gold { get; set; }
    public string? items { get; set; }
}