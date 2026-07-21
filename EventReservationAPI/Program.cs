using EventReservationAPI.Data;
using EventReservationAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string"
        + "'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    int maxRetries = 5;
    for (int retry = 1; retry <= maxRetries; retry++)
    {
        try
        {
            context.Database.Migrate();
            Console.WriteLine("Міграції успішно застосовано!");
            break; 
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Спроба {retry} провалилася. База даних ще не готова...");
            if (retry == maxRetries)
            {
                throw;
            }
            System.Threading.Thread.Sleep(3000);
        }
    }
}
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/events", async (Event newEvent, AppDbContext dbContext) =>
{
    newEvent.CreatedAt = DateTime.UtcNow;

    dbContext.Events.Add(newEvent);
    await dbContext.SaveChangesAsync();

    return Results.Created($"/events/{newEvent.Id}", newEvent);
});

app.MapGet("/events/{id}", async (int id, AppDbContext dbContext) =>
    await dbContext.Events.FindAsync(id)
        is Event existingEvent
            ? Results.Ok(existingEvent)
            : Results.NotFound());

app.MapPut("/events/{id}", async (int id, Event inputEvent, AppDbContext dbContext) =>
{
    var existingEvent = await dbContext.Events.FindAsync(id);

    if (existingEvent is null) return Results.NotFound();

    existingEvent.Name = inputEvent.Name;
    existingEvent.Description = inputEvent.Description;
    existingEvent.Location = inputEvent.Location;
    existingEvent.StartsAt = inputEvent.StartsAt;
    existingEvent.Capacity = inputEvent.Capacity;

    await dbContext.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/events/{id}", async (int id, AppDbContext dbContext) =>
{
    if (await dbContext.Events.FindAsync(id) is Event existingEvent)
    {
        dbContext.Events.Remove(existingEvent);
        await dbContext.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});

app.MapGet("/events", async (AppDbContext dbContext) =>
    await dbContext.Events.ToListAsync());

app.Run();