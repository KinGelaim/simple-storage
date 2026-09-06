/* 
 * Небольшое тестовое приложение для проверки работы SimpleStorage с сообщением превышающим установленный лимит
 */
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
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

var byteCount = 4000;
var bytes = new byte[byteCount];
RandomNumberGenerator.Fill(bytes);

var userName = Convert.ToBase64String(bytes);
var message = $"SET user:1 {{\"Id\": 123, \"UserName\": \"{userName}\", \"CreatedAt\": \"2026-07-14T20:42:14\"}}\r\n";
var data = Encoding.UTF8.GetBytes(message);

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
        await clientSocket.SendAsync(data, SocketFlags.None);
        Console.Write($"Отправлено сообщение длиной: {data.Length}\r\n");

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
