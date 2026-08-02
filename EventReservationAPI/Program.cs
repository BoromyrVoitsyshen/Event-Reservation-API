using EventReservationAPI.Data;
using EventReservationAPI.Entities;
using EventReservationAPI.Infrastructure;
using EventReservationAPI.Services;
using EventReservationAPI.Settings;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Runtime.CompilerServices;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddHostedService<ReservationCleanupService>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http, 
        Scheme = "bearer",
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var dbSettings = builder.Configuration
    .GetSection("DatabaseSettings")
    .Get<DatabaseSettings>() ?? new DatabaseSettings();

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string"
        + "'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
    sqlOptions.EnableRetryOnFailure(
        maxRetryCount: dbSettings.MaxRetryCount,
        maxRetryDelay: TimeSpan.FromSeconds(dbSettings.MaxDelayInSeconds),
        errorNumbersToAdd: null);
    }));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] 
                ?? throw new InvalidOperationException("JWT secret key is not configured.")))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    policy.RequireRole(User.Roles.Admin));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    context.Database.Migrate();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    string adminEmail = "admin@eventapi.com";
    if (!dbContext.Users.Any(u => u.Email == adminEmail))
    {
        var adminUser = new User
        {
            Name = "System Administrator",
            Email = adminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("AdminSecret123!"),
            Role = "Admin" 
        };
        dbContext.Users.Add(adminUser);
        dbContext.SaveChanges();
        Console.WriteLine($"[SEEDING] Successfully created admin user: {adminEmail}");
    }

    //if (dbContext.Events.Any())
    //{
    //    dbContext.Events.RemoveRange(dbContext.Events);
    //    dbContext.SaveChanges();
    //}

    if (!dbContext.Events.Any())
    {
        var events = new List<Event>
        {
           new Event { 
               Name = ".NET Conf 2026", 
               Description = "Biggest conference for .NET developers", 
               Location = "Kyiv", 
               StartsAt = DateTime.UtcNow.AddDays(10), 
               Capacity = 500, 
               CreatedAt = DateTime.UtcNow, 
               AvailableSeats = 500 
           },
            new Event { 
                Name = "Rock Festival", 
                Description = "Rock music festival", 
                Location = "Lviv", 
                StartsAt = DateTime.UtcNow.AddMonths(2), 
                Capacity = 2000, 
                CreatedAt = DateTime.UtcNow, 
                AvailableSeats = 2000 
            },
            new Event { 
                Name = "Local Art Exhibit", 
                Description = "Exhibition of contemporary art (already passed)", 
                Location = "Odesa", 
                StartsAt = DateTime.UtcNow.AddDays(-5), 
                Capacity = 50, 
                CreatedAt = DateTime.UtcNow.AddDays(-20), 
                AvailableSeats = 0 
            },
            new Event { 
                Name = "Kyiv Marathon", 
                Description = "Charity running marathon", 
                Location = "Kyiv", 
                StartsAt = DateTime.UtcNow.AddDays(20), 
                Capacity = 1000, 
                CreatedAt = DateTime.UtcNow, 
                AvailableSeats = 1000 
            },
            new Event { 
                Name = "Startup Pitch Meetup", 
                Description = "Presentation of ideas to investors", 
                Location = "Kharkiv", 
                StartsAt = DateTime.UtcNow.AddDays(2), 
                Capacity = 100, 
                CreatedAt = DateTime.UtcNow, 
                AvailableSeats = 100 
            },
            new Event { 
                Name = "Jazz Night", 
                Description = "Evening of live jazz music", 
                Location = "Lviv", 
                StartsAt = DateTime.UtcNow.AddDays(15), 
                Capacity = 150, 
                CreatedAt = DateTime.UtcNow, 
                AvailableSeats = 150 
            },
            new Event { 
                Name = "C# Masterclass", 
                Description = "Intensive on architecture and patterns", 
                Location = "Online", 
                StartsAt = DateTime.UtcNow.AddDays(5), 
                Capacity = 300, 
                CreatedAt = DateTime.UtcNow, 
                AvailableSeats = 300 
            },
            new Event { 
                Name = "Food Tasting Fair", 
                Description = "Festival of street food", 
                Location = "Kyiv", 
                StartsAt = DateTime.UtcNow.AddDays(8), 
                Capacity = 400, 
                CreatedAt = DateTime.UtcNow, 
                AvailableSeats = 400 
            }
        };

        dbContext.Events.AddRange(events);
        dbContext.SaveChanges();
        Console.WriteLine($"[SEEDING] Successfully added {events.Count} events for testing.");
    }
}
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();


app.MapPost("/events", async (
    EventInputDto dto,
    IValidator<EventInputDto> validator,
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
})
.RequireAuthorization("AdminOnly");

app.MapGet("/events/{id}", async (
    int id,
    IEventService eventService) =>
{
    var existingEvent = await eventService.GetEventAsync(id);
    return existingEvent is not null 
        ? Results.Ok(existingEvent) 
        : Results.NotFound();
})
.RequireAuthorization();

app.MapPut("/events/{id}", async (
    int id,
    EventInputDto dto,
    IValidator<EventInputDto> validator,
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
})
.RequireAuthorization("AdminOnly");

app.MapDelete("/events/{id}", async (
    int id, 
    IEventService eventService) =>
{
    bool isDeleted = await eventService.DeleteEventAsync(id);
    return isDeleted
        ? Results.NoContent()
        : Results.NotFound();
})
.RequireAuthorization("AdminOnly");

app.MapGet("/events", async (
    [AsParameters] EventFilterDto filter,
    IEventService eventService) =>
    {
        var events = await eventService.GetEventsAsync(filter);
        return Results.Ok(events);
    })
.RequireAuthorization();

app.MapPost("/register", async (
    UserRegisterDto userInput, 
    IValidator<UserRegisterDto> validator,
    IAuthService authService) =>
    {
        var validationResult = await validator.ValidateAsync(userInput);
        if (!validationResult.IsValid) 
            return Results.ValidationProblem(validationResult.ToDictionary());

        var user = await authService.RegisterUserAsync(userInput);
        if (user is null)
            return Results.Conflict(new { Message = "User with this email already exists." });

        return Results.Ok(new {
            user.Id,
            user.Email,
            user.Name,
            user.Role
        });
    });

app.MapPost("/login", async (
    UserLoginDto userInput,
    IAuthService authService) =>
    {
        var token = await authService.LoginUserAsync(userInput);
        if (token is null)
            return Results.Unauthorized();

        return Results.Ok(new { Token = token });
    });

app.MapPost("/reservations", async (
    ReservationCreateDto dto,
    IValidator<ReservationCreateDto> validator,
    IReservationService reservationService, 
    ClaimsPrincipal user) =>
    {
        var validationResult = await validator.ValidateAsync(dto);

        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if(string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Results.Unauthorized();
        }

        var reservation = await reservationService.CreateReservationAsync(userId, dto);

        return Results.Created($"/reservations/{reservation.Id}", reservation);
    })
    .RequireAuthorization();

app.MapPost("/reservations/{id}/confirm", async (
    int id,
    IReservationService reservationService,
    ClaimsPrincipal user) =>
{
    var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

    if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
    {
        return Results.Unauthorized();
    }

    var reservation = await reservationService.ConfirmReservationAsync(userId, id);

    return Results.Ok(reservation);
})
.RequireAuthorization();

app.MapPost("/reservations/{id}/cancel", async (
    int id,
    IReservationService reservationService,
    ClaimsPrincipal user) =>
{
    var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

    if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
    {
        return Results.Unauthorized();
    }

    var reservation = await reservationService.CancelReservationAsync(userId, id);

    return Results.Ok(reservation);
})
.RequireAuthorization();

app.Run();