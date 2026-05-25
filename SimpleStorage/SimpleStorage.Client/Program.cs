/* 
 * Небольшое тестовое приложение для проверки работы SimpleStorage
 */
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
var messages = new string[] { "SET user:1 data", "GET user:1", "GET user:2", "DELETE user:3", "DELETE" };

using var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
try
{
    await clientSocket.ConnectAsync(IPAddress.Parse(serverIp), serverPort);
    Console.WriteLine("Подключено к серверу!");
    Console.WriteLine("Для завершения нажмите Ctrl+C");

    while (!shouldStop)
    {
        var message = messages[Random.Shared.Next(messages.Length)];
        var data = Encoding.UTF8.GetBytes(message);

        await clientSocket.SendAsync(data, SocketFlags.None);
        Console.WriteLine($"Отправлено сообщение: {message}");

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
