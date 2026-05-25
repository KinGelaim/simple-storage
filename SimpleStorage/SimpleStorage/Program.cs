using SimpleStorage.Server;

var ip = "127.0.0.1";
var port = 8080;

var tcpServer = new TcpServer(ip, port);
await tcpServer.StartAsync();

Console.ReadKey();
