using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;

namespace SimpleStorage.Client.Benchmark;

internal sealed class LoadTestClient(string host, int port)
{
    private readonly string _host = host;
    private readonly int _port = port;
    private readonly ConcurrentBag<long> _latenciesTicks = [];

    private long _successfulRequests;
    private long _failedRequests;
    private long _totalBytesSent;
    private long _totalBytesReceived;

    public async Task RunLoadTestAsync(
        int numberOfConnections,
        int requestsPerConnection,
        byte[] messageData)
    {
        Console.WriteLine($"=== МАКСИМАЛЬНАЯ НАГРУЗКА (одно сообщение) ===");
        Console.WriteLine($"Соединений: {numberOfConnections}");
        Console.WriteLine($"Запросов на соединение: {requestsPerConnection}");
        Console.WriteLine($"Размер сообщения: {messageData.Length} байт");

        var stopwatch = Stopwatch.StartNew();

        // Создаем список для хранения всех задач-соединений
        var connectionTasks = new List<Task>(numberOfConnections);

        // Запускаем все соединения как асинхронные задачи
        for (var i = 0; i < numberOfConnections; i++)
        {
            connectionTasks.Add(RunConnectionAsync(requestsPerConnection, messageData));
        }

        // Асинхронно ждем, пока ВСЕ задачи завершатся
        await Task.WhenAll(connectionTasks);

        stopwatch.Stop();

        PrintResults(stopwatch.Elapsed);
    }

    public async Task RunLoadTestAsync(
        int numberOfConnections,
        int requestsPerConnection,
        byte[][] messagesData)
    {
        Console.WriteLine($"=== МАКСИМАЛЬНАЯ НАГРУЗКА (несколько сообщений) ===");
        Console.WriteLine($"Соединений: {numberOfConnections}");
        Console.WriteLine($"Запросов на соединение: {requestsPerConnection}");

        var messagesCount = messagesData.Length;
        Console.WriteLine($"Кол-во сообщений: {messagesCount}");

        var messagesLengths = messagesData.Select(messageData => messageData.Length);
        Console.WriteLine($"Размер сообщений: {messagesLengths.Sum()} ({string.Join('+', messagesLengths)}) байт");

        var stopwatch = Stopwatch.StartNew();

        // Создаем список для хранения всех задач-соединений
        var connectionTasks = new List<Task>(numberOfConnections);

        // Запускаем все соединения как асинхронные задачи
        var connections = numberOfConnections / messagesCount;
        for (var i = 0; i < connections; i++)
        {
            foreach (var messageData in messagesData)
            {
                connectionTasks.Add(RunConnectionAsync(requestsPerConnection, messageData));
            }
        }

        // Асинхронно ждем, пока ВСЕ задачи завершатся
        await Task.WhenAll(connectionTasks);

        stopwatch.Stop();

        PrintResults(stopwatch.Elapsed);
    }

    private async Task RunConnectionAsync(int requests, byte[] messageData)
    {
        // 'using' гарантирует, что сокет будет закрыт
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.NoDelay = true; // Важная опция для тестов задержки

        try
        {
            // Асинхронное подключение
            await socket.ConnectAsync(_host, _port);

            // Используем MemoryPool для эффективной работы с памятью
            using var receiveMemoryOwner = MemoryPool<byte>.Shared.Rent(8192);
            var receiveMemory = receiveMemoryOwner.Memory;
            var sendMemory = new ReadOnlyMemory<byte>(messageData);

            for (var i = 0; i < requests; i++)
            {
                var requestStart = Stopwatch.GetTimestamp();

                // Отправка данных
                await socket.SendAsync(sendMemory, SocketFlags.None);
                Interlocked.Add(ref _totalBytesSent, sendMemory.Length);

                // Получение данных
                var bytesReceived = await socket.ReceiveAsync(receiveMemory, SocketFlags.None);
                if (bytesReceived == 0)
                {
                    // Сервер закрыл соединение
                    Interlocked.Increment(ref _failedRequests);
                    break;
                }

                var requestEnd = Stopwatch.GetTimestamp();
                _latenciesTicks.Add(requestEnd - requestStart);

                Interlocked.Add(ref _totalBytesReceived, bytesReceived);
                Interlocked.Increment(ref _successfulRequests);
            }
        }
        catch
        {
            // Если соединение упало, считаем все запросы неудачными
            Interlocked.Add(ref _failedRequests, requests);
        }
    }

    private void PrintResults(TimeSpan elapsed)
    {
        Console.WriteLine("\n=== РЕЗУЛЬТАТЫ ===");
        Console.WriteLine($"Время: {elapsed.TotalSeconds:F3} сек");
        Console.WriteLine($"Успешных запросов: {_successfulRequests:N0}");
        Console.WriteLine($"Неудачных запросов: {_failedRequests:N0}");
        Console.WriteLine();

        if (elapsed.TotalSeconds > 0)
        {
            var rps = _successfulRequests / elapsed.TotalSeconds;
            Console.WriteLine($"RPS: {rps:F2}");
            Console.WriteLine($"Пропускная способность: {(_totalBytesSent + _totalBytesReceived) / elapsed.TotalSeconds / 1024.0 / 1024.0:F2} МБ/сек");
            Console.WriteLine();
        }

        if (!_latenciesTicks.IsEmpty)
        {
            var latenciesMs = _latenciesTicks
                .Select(ticks => ticks * 1000.0 / Stopwatch.Frequency)
                .OrderBy(x => x)
                .ToList();

            Console.WriteLine($"Задержка мин: {latenciesMs.First():F2} мс");
            Console.WriteLine($"Задержка макс: {latenciesMs.Last():F2} мс");
            Console.WriteLine($"Задержка средняя: {latenciesMs.Average():F2} мс");
            Console.WriteLine($"Задержка P50: {GetPercentile(latenciesMs, 50):F2} мс");
            Console.WriteLine($"Задержка P95: {GetPercentile(latenciesMs, 95):F2} мс");
            Console.WriteLine($"Задержка P99: {GetPercentile(latenciesMs, 99):F2} мс");
        }
    }

    private static double GetPercentile(List<double> sorted, double percentile)
    {
        var index = (int)Math.Ceiling(sorted.Count * percentile / 100.0) - 1;
        return sorted[Math.Max(0, Math.Min(sorted.Count - 1, index))];
    }
}