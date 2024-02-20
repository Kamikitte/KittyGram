using System.Collections.Concurrent;

namespace Api.Services;

public sealed class TooManyRequestsException : Exception;

public class DoSGuard
{
    public ConcurrentDictionary<string, int> RequestControlDict { get; set; } = new();

    public void CheckRequest(string? token)
    {
        if (token == null)
        {
            return;
        }

        var dtn = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");

        var key = $"{token}_{dtn}";

        if (RequestControlDict.ContainsKey(key))
        {
            var requests = RequestControlDict.TryGetValue(key, out var t) ? t : 0;
            if (requests > 10)
            {
                throw new TooManyRequestsException();
            }

            var newRequest = requests + 1;
            RequestControlDict.TryUpdate(key, newRequest, requests);
        }

        RequestControlDict.TryAdd(key, 0);
    }
}