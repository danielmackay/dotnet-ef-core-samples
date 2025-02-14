var builder = DistributedApplication.CreateBuilder(args);

var password = builder.AddParameter("password", secret: true, value: "yourStrong(!)Password");

builder
    .AddSqlServer("sql", port: 1700, password: password)
    .WithLifetime(ContainerLifetime.Persistent);

builder.Build().Run();