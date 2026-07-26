using EventReservationAPI.Data;
using EventReservationAPI.Entities;
using EventReservationAPI.Infrastructure;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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

    context.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();


app.MapPost("/events", async (
    InputEventDto dto,
    IValidator<InputEventDto> validator,
    AppDbContext dbContext,
    ILogger<Program> logger) =>
{
    var validationResult = await validator.ValidateAsync(dto);

    if (!validationResult.IsValid)
    {
        logger.LogWarning("Validation failed for event creation: {Errors}", validationResult.Errors);
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

    logger.LogInformation("Event created successfully with ID: {EventId}", createdEvent.Id);
    return Results.Created($"/events/{createdEvent.Id}", createdEvent);
});

app.MapGet("/events/{id}", async (
    int id,
    AppDbContext dbContext,
    ILogger<Program> logger) =>
{
    if (await dbContext.Events.FindAsync(id) is Event existingEvent)
    {
        logger.LogInformation("Event retrieved successfully with ID: {EventId}", id);
        return Results.Ok(existingEvent);
    }
    logger.LogWarning("Event not found with ID: {EventId}", id);
    return Results.NotFound();
});

app.MapPut("/events/{id}", async (
    int id,
    InputEventDto dto,
    IValidator<InputEventDto> validator,
    AppDbContext dbContext,
    ILogger<Program> logger) =>
{
    var validationResult = await validator.ValidateAsync(dto);

    if (!validationResult.IsValid)
    {
        logger.LogWarning("Validation failed for event update: {Errors}", validationResult.Errors);
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

        logger.LogInformation("Event updated successfully with ID: {EventId}", id);
        return Results.Ok(existingEvent);
    }

    logger.LogWarning("Event not found with ID: {EventId}", id);
    return Results.NotFound();
});

app.MapDelete("/events/{id}", async (
    int id, 
    AppDbContext dbContext, 
    ILogger<Program> logger) =>
{
    if (await dbContext.Events.FindAsync(id) is Event existingEvent)
    {
        dbContext.Events.Remove(existingEvent);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Event deleted successfully with ID: {EventId}", id);
        return Results.NoContent();
    }

    logger.LogWarning("Event not found with ID: {EventId}", id);
    return Results.NotFound();
});

app.MapGet("/events", async (AppDbContext dbContext, ILogger<Program> logger) =>
    {
        var events = await dbContext.Events.ToListAsync();
        logger.LogInformation("All events retrieved successfully.");
        return Results.Ok(events);
    });

app.Run();