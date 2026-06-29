using System.Net.Sockets;
using System.Text;

namespace SimpleStorage.Client.LoadTesting;

internal sealed class TcpClient(string host, int port) : IDisposable
{
    private readonly string _host = host;
    private readonly int _port = port;
    private System.Net.Sockets.TcpClient? _client;
    private NetworkStream? _stream;

    public async Task ConnectAsync()
    {
        _client = new System.Net.Sockets.TcpClient();
        await _client.ConnectAsync(_host, _port);
        _stream = _client.GetStream();
    }

    public async Task<string> SetAsync(string key, string value)
    {
        if (_stream is null)
        {
            return string.Empty;
        }

        var command = $"SET {key} {value}\r\n";
        var bytes = Encoding.UTF8.GetBytes(command);
        await _stream.WriteAsync(bytes);

        return await ReadResponseAsync();
    }

    public async Task<string> GetAsync(string key)
    {
        if (_stream is null)
        {
            return string.Empty;
        }

        var command = $"GET {key}\r\n";
        var bytes = Encoding.UTF8.GetBytes(command);
        await _stream.WriteAsync(bytes);

        return await ReadResponseAsync();
    }

    private async Task<string> ReadResponseAsync()
    {
        if (_stream is null)
        {
            return string.Empty;
        }

        var buffer = new byte[1024];
        var byteCount = await _stream.ReadAsync(buffer);
        return Encoding.UTF8.GetString(buffer, 0, byteCount).Trim();
    }

    public void Dispose()
    {
        _stream?.Dispose();
        _client?.Close();
        _client?.Dispose();
    }
}