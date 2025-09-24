using System.Collections.Concurrent;

namespace LearnKazakh.API;

public sealed class PresenceTracker
{
    private readonly ConcurrentDictionary<string, byte> _presenceDictionary = new();

    public bool UserConnected(string connectionId)
    {
        return _presenceDictionary.TryAdd(connectionId, 1);
    }

    public bool UserDisconnected(string connectionId)
    {
        return _presenceDictionary.TryRemove(connectionId, out _);
    }

    public int GetOnlineUsersCount()
    {
        return _presenceDictionary.Count;
    }

    public bool GetUserConnectionStatus(string connectionId)
    {
        return _presenceDictionary.ContainsKey(connectionId);
    }

    public IReadOnlyCollection<string> GetSnapshot()
    {
        return _presenceDictionary.Keys.ToList().AsReadOnly();
    }
}
