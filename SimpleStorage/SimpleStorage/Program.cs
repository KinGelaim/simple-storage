using SimpleStorage.Server;
using SimpleStorage.Services;
using SimpleStorage.Storage;
using SimpleStorage.Workers;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) =>
{
    cts.Cancel();
    e.Cancel = true;
    Console.WriteLine("Получена команда на завершение. Дожидаемся завершения работы сервера....");
};
Console.WriteLine("Для завершения нажмите Ctrl+C");

var store = new SimpleStore();
var commandChannelService = new CommandChannelService();
var worker = new StorageCommandWorker(commandChannelService, store);

_ = Task.Run(() => worker.ProcessCommandsAsync(cts.Token));
_ = Task.Run(() => worker.ProcessCommandsAsync(cts.Token));

var ip = "127.0.0.1";
var port = 8080;
var tcpServer = new TcpServer(ip, port, commandChannelService);
await tcpServer.StartAsync(cts.Token);

Console.WriteLine("Сервер завершил работу");
Console.ReadKey();
