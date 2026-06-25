/* 
 * Небольшое тестовое приложение для проверки отправки команды по частям SimpleStorage
 */
using SimpleStorage.Client.PartialSending;

var client = new PartialTestClient();
await client.ConnectAsync("127.0.0.1", 8080);

Console.WriteLine("\n--- Симуляция частичной отправки ---");

// Подгатавливаем сообщение
var key = $"user:{Random.Shared.Next(1000, 9999)}";
var value = $"data_for_{key}_{Guid.NewGuid()}";
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
