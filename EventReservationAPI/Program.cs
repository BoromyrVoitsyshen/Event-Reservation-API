using EventReservationAPI.Data;
using EventReservationAPI.Entities;
using EventReservationAPI.Infrastructure;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

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

app.UseExceptionHandler();


app.MapPost("/events", async (
    InputEventDto dto,
    IValidator<InputEventDto> validator,
    AppDbContext dbContext) =>
{
    var validationResult = await validator.ValidateAsync(dto);

    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var createdEvent = new Event
    {
        Name = dto.Name,
        Description = dto.Description,
        Location = dto.Location,
        StartsAt = dto.StartsAt,
        Capacity = dto.Capacity,
        CreatedAt = DateTime.UtcNow
    };

    dbContext.Events.Add(createdEvent);
    await dbContext.SaveChangesAsync();

    return Results.Created($"/events/{createdEvent.Id}", createdEvent);
});

app.MapGet("/events/{id}", async (int id, AppDbContext dbContext) =>
    await dbContext.Events.FindAsync(id) is Event existingEvent
            ? Results.Ok(existingEvent)
            : Results.NotFound());

app.MapPut("/events/{id}", async (
    int id,
    InputEventDto dto,
    IValidator<InputEventDto> validator,
    AppDbContext dbContext) =>
{
    var validationResult = await validator.ValidateAsync(dto);

    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }

    if (await dbContext.Events.FindAsync(id) is Event existingEvent)
    {
        existingEvent.Name = dto.Name;
        existingEvent.Description = dto.Description;
        existingEvent.Location = dto.Location;
        existingEvent.StartsAt = dto.StartsAt;
        existingEvent.Capacity = dto.Capacity;

        await dbContext.SaveChangesAsync();

        return Results.Ok(existingEvent);
    }

    return Results.NotFound();
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