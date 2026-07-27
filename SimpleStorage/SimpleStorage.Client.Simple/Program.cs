/* 
 * Небольшое тестовое приложение для проверки работы SimpleStorage
 */
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;

var shouldStop = false;
Console.CancelKeyPress += (sender, e) =>
{
    shouldStop = true;
    e.Cancel = true;
    Console.WriteLine("Получена команда на завершение. Дожидаемся завершения цикла...");
};

var serverIp = "127.0.0.1";
var serverPort = 8080;
var messages = new string[] {
    "SET user:1 {\"Id\": 123, \"UserName\": \"Misha\", \"CreatedAt\": \"2026-07-14T20:42:14\"}\r\n",
    "SET user:1 data\r\n",
    "GET user:1\r\n",
    "GET user:2\r\n",
    "DELETE user:3\r\n",
    "DELETE\r\n"
};

var bufferSize = 1024;
var responseBuffer = ArrayPool<byte>.Shared.Rent(bufferSize);

using var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
try
{
    await clientSocket.ConnectAsync(IPAddress.Parse(serverIp), serverPort);
    Console.WriteLine("Подключено к серверу!");
    Console.WriteLine("Для завершения нажмите Ctrl+C\n");

    while (!shouldStop)
    {
        var message = messages[Random.Shared.Next(messages.Length)];
        var data = Encoding.UTF8.GetBytes(message);

        await clientSocket.SendAsync(data, SocketFlags.None);
        Console.Write($"Отправлено сообщение: {message}");

        var bytesReceived = await clientSocket.ReceiveAsync(responseBuffer, SocketFlags.None);
        var responseMessage = Encoding.UTF8.GetString(responseBuffer, 0, bytesReceived);
        Console.Write($"Получено сообщение: {responseMessage}");
        Console.WriteLine();

        await Task.Delay(1000);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Ошибка подключения: {ex.Message}");
}
finally
{
    clientSocket.Shutdown(SocketShutdown.Both);
    clientSocket.Close();
    Console.WriteLine("Соединение закрыто");
}

Console.ReadKey();
