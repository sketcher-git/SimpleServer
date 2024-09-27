using Application.Data.OnlineData;
using Domain.Items;
using Domain.Players;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Application.Data;

public class OnlineCacheController
{
    private static readonly Lazy<OnlineCacheController> _instance = new Lazy<OnlineCacheController>(() => new OnlineCacheController());

    private readonly Dictionary<Guid, Dictionary<Guid, Item>> _itemOnlineCacheMap;
    private readonly Dictionary<Guid, PlayerOnlineCache> _playerOnlineCacheMap;

    public static OnlineCacheController Instance => _instance.Value;

    private OnlineCacheController()
    {
        _itemOnlineCacheMap = new Dictionary<Guid, Dictionary<Guid, Item>>();
        _playerOnlineCacheMap = new Dictionary<Guid, PlayerOnlineCache>();
    }

    internal Item? GetItemByDataId(Guid playerId, int dataId)
    {
        var itemMap = GetItemMapByPlayerId(playerId);
        if (itemMap == null
        || itemMap.Count == 0)
            return null;

        foreach (var item in itemMap)
        {
            if (item.Value.DataId == dataId)
                return item.Value;
        }

        return null;
    }

    internal Item? GetItemByItemId(Guid playerId, Guid itemId)
    {
        var itemMap = GetItemMapByPlayerId(playerId);
        if (itemMap == null
        || itemMap.Count == 0)
            return null;

        TryGetCacheTableValue(itemMap, itemId, out var item);
        return item;
    }

    public Dictionary<Guid, Item>? GetItemMapByPlayerId(Guid playerId)
    {
        TryGetCacheTableValue(_itemOnlineCacheMap, playerId, out var itemMap);
        return itemMap;
    }

    public PlayerOnlineCache? GetPlayerOnlineCacheByPlayerId(Guid playerId)
    {
        TryGetCacheTableValue(_playerOnlineCacheMap, playerId, out var player);
        return player;
    }

    public Player? GetPlayerRecordByPlayerId(Guid playerId)
    {
        if(!TryGetCacheTableValue(_playerOnlineCacheMap, playerId, out var player))
                return null;

        return player.Record;
    }

    public void RegisterItemMap(Guid playerId, Dictionary<Guid, Item> itemMap)
    {
        _itemOnlineCacheMap.Add(playerId, itemMap);
    }

    internal void RegisterPlayer(PlayerOnlineCache player)
    {
        _playerOnlineCacheMap.Add(player.Id, player);
    }

    internal bool RemoveItemByItemId(Guid playerId, Guid itemId)
    {
        var itemMap = GetItemMapByPlayerId(playerId);
        if (itemMap == null
        || itemMap.Count == 0)
            return false;

        return itemMap.Remove(itemId);
    }

    private bool TryGetCacheTableValue<TKey, TValue>(Dictionary<TKey, TValue> collectionTable, TKey key, out TValue value)
        where TKey : notnull
    {
        value = default(TValue);
        ref var valueOrNull = ref CollectionsMarshal.GetValueRefOrNullRef(collectionTable, key);
        if (Unsafe.IsNullRef(valueOrNull))
            return false;

        value = valueOrNull;
        return true;
    }

    internal void ClearAllCacheByPlayerId(Guid playerId)
    {
        _itemOnlineCacheMap.Remove(playerId);
        _playerOnlineCacheMap.Remove(playerId);
    }

    internal bool UnregisterItemMap(Guid playerId)
    {
        return _itemOnlineCacheMap.Remove(playerId);
    }

    internal bool UnregisterPlayer(Guid playerId)
    {
        return _playerOnlineCacheMap.Remove(playerId);
    }
}