using DevMatch.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<IssueCatalogSyncWorker>();
var host = builder.Build();
host.Run();
