using System.Collections.Concurrent;

namespace BuildingSoftwareLabaratoryWork.Common;

internal static class State
{
    internal static ConcurrentDictionary<string, int> state = new();
}