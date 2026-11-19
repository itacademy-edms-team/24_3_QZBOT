using System.Threading.Channels;
using Backend.Models;

namespace Backend.Services;

public sealed class RequestLogQueue
{
    private readonly Channel<HttpRequestLog> channel = Channel.CreateBounded<HttpRequestLog>(
        new BoundedChannelOptions(1024) { SingleReader = true, FullMode = BoundedChannelFullMode.Wait });

    public bool TryEnqueue(HttpRequestLog entry) => channel.Writer.TryWrite(entry);

    internal IAsyncEnumerable<HttpRequestLog> ReadAllAsync(CancellationToken cancellationToken) =>
        channel.Reader.ReadAllAsync(cancellationToken);

    internal void Complete() => channel.Writer.TryComplete();
}
