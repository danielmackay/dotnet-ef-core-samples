var builder = DistributedApplication.CreateBuilder(args);

var password = builder.AddParameter("password", secret: true, value: "yourStrong(!)Password");

builder
    .AddSqlServer("sql", port: 1700, password: password)
    // SQL Server 2025 is needed for the json type, JSON_CONTAINS and compatibility level 170.
    .WithImageTag("2025-latest")
    .WithLifetime(ContainerLifetime.Persistent);

// Only the CosmosDb sample uses this. RunAsEmulator runs the Linux-based (vNext) emulator, which is
// native on ARM64. Common/DbConnectionFactory.CreateCosmos() connects to the fixed gateway port over
// plain HTTP, so the emulator doesn't get Aspire's HTTPS developer certificate
// (WithoutHttpsCertificate is an evaluation API: ASPIRECERTIFICATES001 is suppressed in AppHost.csproj).
builder
    .AddAzureCosmosDB("cosmos")
    .RunAsEmulator(emulator => emulator
        .WithGatewayPort(8081)
        .WithDataExplorer(1234)
        .WithoutHttpsCertificate()
        .WithLifetime(ContainerLifetime.Persistent));

builder.Build().Run();
