using System.Collections.Concurrent;

namespace BuildingSoftwareLabaratoryWork.Common;

internal static class State
{
    internal static ConcurrentDictionary<string, int> state = new();

    static State()
    {
        state.GetOrAdd("first", 12);
        state.GetOrAdd("second", 13);
        state.GetOrAdd("third", 14);
    }
}