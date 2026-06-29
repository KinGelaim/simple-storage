using NBomber.CSharp;
using SimpleStorage.Client.LoadTesting;

var host = "127.0.0.1";
var port = 8080;

var setScenario = Scenario.Create(
    "test_scenario",
    async context =>
    {
        var setStep = await Step.Run("set_command", context, async () =>
        {
            using var client = new TcpClient(host, port);
            try
            {
                await client.ConnectAsync();

                var key = $"key_{Random.Shared.Next(1000, 9999)}";
                var value = Guid.NewGuid().ToString();
                await client.SetAsync(key, value);

                return Response.Ok();
            }
            catch (Exception ex)
            {
                return Response.Fail(message: ex.Message);
            }
        });

        return setStep;
    })
    .WithWarmUpDuration(TimeSpan.FromSeconds(7))
    .WithLoadSimulations(
        Simulation.Inject(
            rate: 100,
            interval: TimeSpan.FromSeconds(1),
            during: TimeSpan.FromSeconds(30))
    );

NBomberRunner
    .RegisterScenarios(setScenario)
    .Run();
