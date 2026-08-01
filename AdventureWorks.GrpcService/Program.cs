/*
 * Copyright (c) 2026 NTTECO.
 *
 * This source code is provided for demonstration and educational purposes only.
 * It is offered "as is", without warranty of any kind, express or implied.
 *
 * The WCF and gRPC code in this repository is intentionally minimal and may not
 * represent production-ready implementations.
 *
 * **The purpose of this repository is to illustrate the development effort and
 * complexity involved in modernizing WCF services to gRPC. NTTECO is referenced
 * only as a modernization usecase category — not as a migration tool, converter,
 * or automation framework.**
 *
 * NTTECO is the Metadata-Driven Object Graph Authority Platform. No NTTECO
 * implementation is included in this repository.
 *
 * For more information about NTTECO, visit https://ntteco.com.
 */
using Microsoft.EntityFrameworkCore;
using AdventureWorks.GrpcService.Data;
using AdventureWorks.GrpcService.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration: prefer environment variable, fallback to appsettings
var connectionString = builder.Configuration.GetConnectionString("AdventureWorks2025")
                       ?? builder.Configuration["ConnectionStrings:AdventureWorks2025"]
                       ?? "Server=.;Database=AdventureWorks2025;Trusted_Connection=True;MultipleActiveResultSets=true";

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services
builder.Services.AddGrpc();
builder.Services.AddHealthChecks();

// Register DbContextFactory for safe short-lived contexts in gRPC handlers
builder.Services.AddDbContextFactory<AdventureWorks2025Context>(options =>
    options.UseSqlServer(connectionString, sql =>
    {
        sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
    }));

// Optional: add CORS if you plan to call gRPC-Web from browsers
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Development diagnostics
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Use CORS only if needed for gRPC-Web clients
app.UseCors("AllowAll");

// Map gRPC services
app.MapGrpcService<GrpcPersonService>();
app.MapGrpcService<GrpcProductService>();
app.MapGrpcService<GrpcWorkOrderService>();

// Health and root info
app.MapHealthChecks("/health");
app.MapGet("/", () => "gRPC server running. Use a gRPC client to call endpoints.");

// Ensure Kestrel supports HTTP/2 (default for gRPC on TLS). If you need explicit config, configure Kestrel in builder.WebHost.ConfigureKestrel.
app.Run();
