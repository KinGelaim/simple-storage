using NBomber.Contracts;
using NBomber.CSharp;
using SimpleStorage.DTO;
using System.Text.Json;

namespace SimpleStorage.Client.LoadTesting;

internal static class Scenarios
{
    public static ScenarioProps CreateSimpleSetScenario(string host, int port) =>
        Scenario.Create(
            "simple_set_scenario",
            async context =>
            {
                var setStep = await Step.Run("set_command", context, async () =>
                {
                    using var client = new TcpClient(host, port);
                    try
                    {
                        await client.ConnectAsync();

                        var key = $"key_{Random.Shared.Next(1000, 9999)}";
                        var value = CreateSimpleValue();
                        await client.SetAsync(key, value);

                        return Response.Ok();
                    }
                    catch (Exception ex)
                    {
                        return Response.Fail(message: ex.Message);
                    }
                });

                return setStep;
            });

    public static ScenarioProps CreateSimpleGetScenario(string host, int port) =>
        Scenario.Create(
            "simple_get_scenario",
            async context =>
            {
                var getStep = await Step.Run("get_command", context, async () =>
                {
                    using var client = new TcpClient(host, port);
                    try
                    {
                        await client.ConnectAsync();

                        var key = $"key_{Random.Shared.Next(1000, 9999)}";
                        await client.GetAsync(key);

                        return Response.Ok();
                    }
                    catch (Exception ex)
                    {
                        return Response.Fail(message: ex.Message);
                    }
                });

                return getStep;
            });

    public static ScenarioProps CreateSimpleSetGetScenario(string host, int port) =>
        Scenario.Create(
            "simple_set_get_scenario",
            async context =>
            {
                using var client = new TcpClient(host, port);

                try
                {
                    await client.ConnectAsync();

                    var key = $"key_{Random.Shared.Next(1000, 9999)}";

                    await Step.Run("set_command", context, async () =>
                    {
                        var value = CreateSimpleValue();
                        var response = await client.SetAsync(key, value);
                        if (response is null)
                        {
                            return Response.Fail();
                        }

                        return Response.Ok();
                    });

                    await Step.Run("get_command", context, async () =>
                    {
                        var response = await client.GetAsync(key);
                        if (response is null)
                        {
                            return Response.Fail();
                        }

                        return Response.Ok();
                    });

                    return Response.Ok();
                }
                catch (Exception ex)
                {
                    return Response.Fail(message: ex.Message);
                }
            });

    public static ScenarioProps CreateSimpleSetGetScenarioWithOneConnection(TcpClient client) =>
        Scenario.Create(
            "simple_set_get_scenario_with_one_connection",
            async context =>
            {
                try
                {
                    var key = $"key_{Random.Shared.Next(1000, 9999)}";

                    await Step.Run("set_command", context, async () =>
                    {
                        var value = CreateSimpleValue();
                        var response = await client.SetAsync(key, value);
                        if (response is null)
                        {
                            return Response.Fail();
                        }

                        return Response.Ok();
                    });

                    await Step.Run("get_command", context, async () =>
                    {
                        var response = await client.GetAsync(key);
                        if (response is null)
                        {
                            return Response.Fail();
                        }

                        return Response.Ok();
                    });

                    return Response.Ok();
                }
                catch (Exception ex)
                {
                    return Response.Fail(message: ex.Message);
                }
            })
            .WithInit(async context => await client.ConnectAsync());

    public static ScenarioProps CreateSimpleSetGetScenarioWithManyManyRequests(
        string host,
        int port,
        int requestsCount) =>
            Scenario.Create(
                "simple_set_get_scenario_with_many_requests",
                async context =>
                {
                    var client = new TcpClient(host, port);

                    try
                    {
                        await client.ConnectAsync();

                        for (var i = 0; i < requestsCount; i++)
                        {
                            var key = $"key_{Random.Shared.Next(1000, 9999)}";

                            await Step.Run("set_command", context, async () =>
                            {
                                var value = CreateSimpleValue();
                                var response = await client.SetAsync(key, value);
                                if (response is null)
                                {
                                    return Response.Fail();
                                }

                                return Response.Ok();
                            });

                            await Step.Run("get_command", context, async () =>
                            {
                                var response = await client.GetAsync(key);
                                if (response is null)
                                {
                                    return Response.Fail();
                                }

                                return Response.Ok();
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        return Response.Fail(message: ex.Message);
                    }

                    return Response.Ok();
                });

    private static string CreateSimpleValue()
    {
        var userProfile = new UserProfile()
        {
            Id = Random.Shared.Next(1000, 9999),
            UserName = "Misha",
            CreatedAt = DateTime.UtcNow
        };
        return JsonSerializer.Serialize(userProfile);
    }
}