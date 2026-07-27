using EventReservationAPI.Data;
using EventReservationAPI.Entities;
using EventReservationAPI.Infrastructure;
using EventReservationAPI.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddScoped<IEventService, EventService>();

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
    IEventService eventService,
    ILogger<Program> logger) =>
{
    var validationResult = await validator.ValidateAsync(dto);

    if (!validationResult.IsValid)
    {
        logger.LogWarning("Validation failed for event creation: {Errors}", validationResult.Errors);
        return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var createdEvent = await eventService.CreateEventAsync(dto);

    return Results.Created($"/events/{createdEvent.Id}", createdEvent);
});

app.MapGet("/events/{id}", async (
    int id,
    IEventService eventService) =>
{
    var existingEvent = await eventService.GetEventAsync(id);
    return existingEvent is not null 
        ? Results.Ok(existingEvent) 
        : Results.NotFound();
});

app.MapPut("/events/{id}", async (
    int id,
    InputEventDto dto,
    IValidator<InputEventDto> validator,
    IEventService eventService,
    ILogger<Program> logger) =>
{
    var validationResult = await validator.ValidateAsync(dto);

    if (!validationResult.IsValid)
    {
        logger.LogWarning("Validation failed for event update: {Errors}", validationResult.Errors);
        return Results.ValidationProblem(validationResult.ToDictionary());
    }

    var updatedEvent = await eventService.UpdateEventAsync(id, dto);

    return updatedEvent is not null 
    ? Results.Ok(updatedEvent) 
    : Results.NotFound();
});

app.MapDelete("/events/{id}", async (
    int id, 
    IEventService eventService) =>
{
    bool isDeleted = await eventService.DeleteEventAsync(id);
    return isDeleted
        ? Results.NoContent()
        : Results.NotFound();
});

app.MapGet("/events", async (
    [AsParameters] FilterEventDto filter,
    IEventService eventService) =>
    {
        var events = await eventService.GetEventsAsync(filter);
        return Results.Ok(events);
    });

app.Run();