using Application.Abstractions.Config;

namespace Application.ConfigEntity;

internal record item_cfg : IDataEntity
{
    public int id { get; init; }
    public string? name { get; init; }
}