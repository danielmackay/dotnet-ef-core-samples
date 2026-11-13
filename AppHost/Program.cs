var builder = DistributedApplication.CreateBuilder(args);

var password = builder.AddParameter("password", secret: true, value: "yourStrong(!)Password");

builder
    .AddSqlServer("sql", port: 1700, password: password)
    // SQL Server 2025 is needed for the json type, JSON_CONTAINS and compatibility level 170.
    .WithImageTag("2025-latest")
    .WithLifetime(ContainerLifetime.Persistent);

builder.Build().Run();
