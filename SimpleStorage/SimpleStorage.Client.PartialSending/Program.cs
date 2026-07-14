/* 
 * Небольшое тестовое приложение для проверки отправки команды по частям SimpleStorage
 */
using SimpleStorage.Client.PartialSending;

var client = new PartialTestClient();
await client.ConnectAsync("127.0.0.1", 8080);

Console.WriteLine("\n--- Симуляция частичной отправки ---");

// Подгатавливаем сообщение
var key = $"user:{Random.Shared.Next(1000, 9999)}";
// lang=json,strict
var value = """{"Id": 123, "UserName": "Misha", "CreatedAt": "2026-07-14T20:42:14"}""";
var command = $"SET {key} {value}\r\n";

// Отправляем по частям
await client.SendPartiallyAsync(command);

// Читаем ответ
var response = await client.ReadResponseAsync();
Console.WriteLine($"[СЕРВЕР]: {response}");

// Отправляем вторую команду, чтобы проверить соединение
await client.SendAsync($"GET {key}\r\n");
response = await client.ReadResponseAsync();
Console.WriteLine($"[СЕРВЕР]: {response}");

Console.WriteLine("\nТест завершён!");
Console.ReadKey();
