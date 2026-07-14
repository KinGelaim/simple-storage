using System.Net.Sockets;
using System.Text;

namespace SimpleStorage.Client.PartialSending;

internal sealed class PartialTestClient : IDisposable
{
    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private StreamReader? _reader;
    private StreamWriter? _writer;

    public async Task ConnectAsync(string host, int port)
    {
        _tcpClient = new TcpClient();
        await _tcpClient.ConnectAsync(host, port);

        _stream = _tcpClient.GetStream();

        // Устанавливаем NoDelay для интерактивности
        _tcpClient.NoDelay = true;

        // Используем StreamReader/Writer для удобной работы с текстовым протоколом
        // Важно: не закрывать базовый поток (leaveOpen: true)
        // Важно: используем UTF8 без BOM, чтобы не отправлять 3 байта (EF BB BF) при создании писателя
        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        _reader = new StreamReader(_stream, encoding, leaveOpen: true);
        _writer = new StreamWriter(_stream, encoding, leaveOpen: true) { AutoFlush = true };

        Console.WriteLine($"Подключено к {host}:{port}");
    }

    /// <summary>
    /// Отправляет сообщение одной командой
    /// </summary>
    public async Task SendAsync(string message)
    {
        if (_writer is null)
        {
            throw new InvalidOperationException("Клиент не подключен");
        }

        Console.WriteLine($"[КЛИЕНТ->]: {message.TrimEnd()}");
        await _writer.WriteAsync(message);
    }

    /// <summary>
    /// Симулирует отправку данных по частям
    /// </summary>
    public async Task SendPartiallyAsync(string message)
    {
        if (_stream is null)
        {
            throw new InvalidOperationException("Клиент не подключен");
        }

        Console.WriteLine($"[КЛИЕНТ->]: Отправка по частям -> {message.TrimEnd()}");

        var data = Encoding.UTF8.GetBytes(message);

        // Разделяем сообщение на 2-3 части для демонстрации
        var part1Length = data.Length / 3;
        var part2Length = data.Length / 3;
        var part3Length = data.Length - part1Length - part2Length;

        // Часть 1
        await _stream.WriteAsync(data.AsMemory(0, part1Length));
        Console.WriteLine($"  ...отправлена часть 1 ({part1Length} байт)");
        await Task.Delay(50); // Небольшая задержка, чтобы сервер успел прочитать

        // Часть 2
        await _stream.WriteAsync(data.AsMemory(part1Length, part2Length));
        Console.WriteLine($"  ...отправлена часть 2 ({part2Length} байт)");
        await Task.Delay(50);

        // Часть 3 (остаток)
        await _stream.WriteAsync(data.AsMemory(part1Length + part2Length, part3Length));
        Console.WriteLine($"  ...отправлена часть 3 ({part3Length} байт)");

        await _stream.FlushAsync();
    }


    /// <summary>
    /// Читает одну строку ответа от сервера
    /// </summary>
    public async Task<string> ReadResponseAsync()
    {
        if (_reader == null)
        {
            throw new InvalidOperationException("Клиент не подключен");
        }

        var response = await _reader.ReadLineAsync();
        return response ?? "[Соединение закрыто]";
    }

    public void Dispose()
    {
        _reader?.Dispose();
        _writer?.Dispose();
        _stream?.Dispose();
        _tcpClient?.Dispose();
    }
}