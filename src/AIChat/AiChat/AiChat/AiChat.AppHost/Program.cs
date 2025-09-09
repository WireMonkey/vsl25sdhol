var builder = DistributedApplication.CreateBuilder(args);

var vectorDB = builder.AddQdrant("vectordb")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEnvironment("QDRANT__SERVICE__ENABLE_CORS", builder.Configuration["Qdrant:EnableCors"]!);

var webApp = builder.AddProject<Projects.AiChat_Web>("aichatweb-app");
webApp
    .WithReference(vectorDB)
    .WaitFor(vectorDB);

builder.Build().Run();
