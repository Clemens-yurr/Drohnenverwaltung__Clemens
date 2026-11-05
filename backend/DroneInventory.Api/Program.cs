using DroneInventory.Api.Data;
using DroneInventory.Api.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<DroneInventoryDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DroneLending")));

var app = builder.Build();

// Apply pending migrations so the SQLite database always matches the model on startup.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DroneInventoryDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/api/drones", () => Results.Ok(Array.Empty<object>()))
    .WithName("GetDrones");

app.MapStudentGroupEndpoints();

app.Run();

// Exposed so integration tests can host the API with WebApplicationFactory<Program>.
public partial class Program { }
