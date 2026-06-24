using SimpleStorage.Server;
using SimpleStorage.Storage;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) =>
{
    cts.Cancel();
    e.Cancel = true;
    Console.WriteLine("Получена команда на завершение. Дожидаемся завершения работы сервера....");
};
Console.WriteLine("Для завершения нажмите Ctrl+C");

var ip = "127.0.0.1";
var port = 8080;
var store = new SimpleStore();

var tcpServer = new TcpServer(ip, port, store);
await tcpServer.StartAsync(cts.Token);

Console.WriteLine("Сервер завершил работу");
Console.ReadKey();
