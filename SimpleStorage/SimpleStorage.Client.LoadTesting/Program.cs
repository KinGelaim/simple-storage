using NBomber.CSharp;
using SimpleStorage.Client.LoadTesting;

var host = "127.0.0.1";
var port = 8080;

var setGetScenario = Scenarios.CreateSimpleSetGetScenario(host, port)
    .WithWarmUpDuration(TimeSpan.FromSeconds(7))
    .WithLoadSimulations(
        Simulation.Inject(
            rate: 100,
            interval: TimeSpan.FromSeconds(1),
            during: TimeSpan.FromSeconds(30))
    );

NBomberRunner
    .RegisterScenarios(setGetScenario)
    .Run();
