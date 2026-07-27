/* 
 * Небольшое тестовое приложение для нагрузочной проверки SimpleStorage
 */
using SimpleStorage.Client.Benchmark;
using System.Globalization;
using System.Text;

Console.WriteLine("Тестовый клиент для нагрузочной проверки");
Console.WriteLine("========================================");
Console.WriteLine();

while (true)
{
    Console.WriteLine("Выберите действие:");
    Console.WriteLine("1. Отправить сообщение БЕЗ обращения к хранилищу");
    Console.WriteLine("2. Отправить сообщение С обращением к хранилищу (SET)");
    Console.WriteLine("3. Отправить сообщения С обращением к хранилищу (GET, SET, DELETE)");
    Console.WriteLine("4. Выход");
    Console.Write("> ");

    var command = Console.ReadKey().KeyChar;
    Console.WriteLine();

    switch (command)
    {
        case '1':
            await SendMessageWithoutStorageAsync();
            break;
        case '2':
            await SendMessageWithStorageAsync();
            break;
        case '3':
            await SendMessagesWithStorageAsync();
            break;
        case '4':
            return;
        default:
            break;
    }
}

static async Task SendMessageWithoutStorageAsync()
{
    Console.WriteLine("Генерируем сообщение для отправки БЕЗ обращения к хранилищу");

    var messageLength = 1024;
    var buffer = new char[messageLength];
    for (var i = 0; i < messageLength - 2; i++)
    {
        buffer[i] = 'X';
    }

    buffer[messageLength - 2] = '\r';
    buffer[messageLength - 1] = '\n';

    var messageData = Encoding.UTF8.GetBytes(buffer);
    await SendMessageAsync(messageData);
}

static async Task SendMessageWithStorageAsync()
{
    Console.WriteLine("Генерируем сообщение для отправки С обращением к хранилищу (SET)");
    var value = CreateSimpleValue();
    var message = $"SET user:1111 {value}\r\n";
    var messageData = Encoding.UTF8.GetBytes(message);
    await SendMessageAsync(messageData);
}

static async Task SendMessageAsync(byte[] messageData)
{
    var client = new LoadTestClient("127.0.0.1", 8080);

    Console.WriteLine("Нажмите Enter для начала теста...");
    Console.ReadLine();

    await client.RunLoadTestAsync(
        numberOfConnections: 100,
        requestsPerConnection: 100,
        messageData: messageData
    );

    Console.WriteLine();
}

static async Task SendMessagesWithStorageAsync()
{
    Console.WriteLine("Генерируем сообщения для отправки С обращением к хранилищу (GET, SET, DELETE)");
    var key = $"user:{Random.Shared.Next(1000, 9999)}";
    var value = CreateSimpleValue();

    var getMessage = $"GET {key}\r\n";
    var setMessage = $"SET {key} {value}\r\n";
    var deleteMessage = $"DELETE {key}\r\n";

    var getMessageData = Encoding.UTF8.GetBytes(getMessage);
    var setMessageData = Encoding.UTF8.GetBytes(setMessage);
    var deleteMessageData = Encoding.UTF8.GetBytes(deleteMessage);

    var messagesData = new byte[][]
    {
        getMessageData,
        setMessageData,
        getMessageData,
        deleteMessageData,
        getMessageData
    };

    await SendMessagesAsync(messagesData);
}

static async Task SendMessagesAsync(byte[][] messagesData)
{
    var client = new LoadTestClient("127.0.0.1", 8080);
    var numberOfConnections = messagesData.Length * 20;
    var requestsPerConnection = 100;

    Console.WriteLine("Нажмите Enter для начала теста...");
    Console.ReadLine();

    await client.RunLoadTestAsync(
        numberOfConnections: numberOfConnections,
        requestsPerConnection: requestsPerConnection,
        messagesData: messagesData
    );

    Console.WriteLine();
}

static string CreateSimpleValue()
{
    var id = Random.Shared.Next(1000, 9999);
    var createdAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
    return $$"""{"Id": {{id}}, "UserName": "{{new string('X', 944)}}", "CreatedAt": "{{createdAt}}"}""";
}