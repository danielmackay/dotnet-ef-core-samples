var builder = DistributedApplication.CreateBuilder(args);

var password = builder.AddParameter("password", secret: true, value: "yourStrong(!)Password");

builder
    .AddSqlServer("sql", port: 1700, password: password)
    // SQL Server 2025 (json type, JSON_CONTAINS, compatibility level 170) plus Full-Text Search,
    // which the stock Linux image doesn't include. The image is built from SqlServer/Dockerfile.
    .WithDockerfile("SqlServer")
    .WithLifetime(ContainerLifetime.Persistent);

builder.Build().Run();
