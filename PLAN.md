# Workout Tracker CRUD API - Implementation Plan

## Overview
Build a portfolio-worthy .NET 8 Web API demonstrating the complete request lifecycle from client request through delivery. Uses ASP.NET Core with SQLite/EF Core for a Workout Tracker application.

## Architecture: Controller-Based API with Clean Architecture

```
WorkoutTracker/
├── WorkoutTracker.Api/           # Controllers, Middleware, Program.cs
├── WorkoutTracker.Core/          # Entities, Interfaces, Enums
├── WorkoutTracker.Application/   # DTOs, Services, Validators
├── WorkoutTracker.Infrastructure/# DbContext, Repositories, Migrations
└── WorkoutTracker.Tests/         # Unit & Integration tests
```

## Data Model

**Workout Entity** (time-series tracking):
- Id, ExerciseName, Category (enum)
- Sets, Reps, Weight, WeightUnit
- PerformedAt (UTC timestamp - key for time-series)
- Duration, Notes, PerceivedExertion (RPE 1-10)
- CreatedAt, UpdatedAt
- ExerciseSets (child collection)

**ExerciseSet Entity**:
- Id, WorkoutId, SetNumber, Reps, Weight, IsWarmup, Notes

## Request Pipeline (Educational Focus)

Custom middleware to demonstrate the full lifecycle:
```
Client Request
    ↓
1. RequestTimingMiddleware (start timer, add correlation ID)
2. ExceptionHandlingMiddleware (global try/catch)
3. RequestLoggingMiddleware (log request details)
4. CORS → Routing → Endpoint Execution
    ↓
5. Model Binding → Validation → Action Filters → Controller
    ↓
6. Response flows back through middleware
    ↓
Client Response (with X-Correlation-Id, X-Response-Time-Ms headers)
```

## Implementation Steps

### Step 1: Create Solution Structure
```bash
dotnet new sln -n WorkoutTracker
dotnet new webapi -n WorkoutTracker.Api -o WorkoutTracker.Api
dotnet new classlib -n WorkoutTracker.Core -o WorkoutTracker.Core
dotnet new classlib -n WorkoutTracker.Application -o WorkoutTracker.Application
dotnet new classlib -n WorkoutTracker.Infrastructure -o WorkoutTracker.Infrastructure
dotnet new xunit -n WorkoutTracker.Tests -o WorkoutTracker.Tests

# Add projects to solution
dotnet sln add WorkoutTracker.Api
dotnet sln add WorkoutTracker.Core
dotnet sln add WorkoutTracker.Application
dotnet sln add WorkoutTracker.Infrastructure
dotnet sln add WorkoutTracker.Tests

# Add project references
cd WorkoutTracker.Api
dotnet add reference ../WorkoutTracker.Application ../WorkoutTracker.Infrastructure
cd ../WorkoutTracker.Application
dotnet add reference ../WorkoutTracker.Core
cd ../WorkoutTracker.Infrastructure
dotnet add reference ../WorkoutTracker.Core
cd ../WorkoutTracker.Tests
dotnet add reference ../WorkoutTracker.Api ../WorkoutTracker.Application ../WorkoutTracker.Core
```

### Step 2: Install NuGet Packages
```bash
# Infrastructure
cd WorkoutTracker.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design

# API
cd ../WorkoutTracker.Api
dotnet add package Swashbuckle.AspNetCore

# Tests
cd ../WorkoutTracker.Tests
dotnet add package Moq
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

### Step 3: Build Core Layer

**WorkoutTracker.Core/Enums/ExerciseCategory.cs**
```csharp
namespace WorkoutTracker.Core.Enums;

public enum ExerciseCategory
{
    Strength = 0,
    Cardio = 1,
    Flexibility = 2,
    Balance = 3,
    Plyometric = 4,
    Other = 5
}
```

**WorkoutTracker.Core/Entities/Workout.cs**
```csharp
namespace WorkoutTracker.Core.Entities;

using WorkoutTracker.Core.Enums;

public class Workout
{
    public int Id { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public ExerciseCategory Category { get; set; }
    public int Sets { get; set; }
    public int Reps { get; set; }
    public decimal? Weight { get; set; }
    public string? WeightUnit { get; set; }
    public DateTime PerformedAt { get; set; }
    public TimeSpan? Duration { get; set; }
    public string? Notes { get; set; }
    public int? PerceivedExertion { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ICollection<ExerciseSet> ExerciseSets { get; set; } = new List<ExerciseSet>();
}
```

**WorkoutTracker.Core/Entities/ExerciseSet.cs**
```csharp
namespace WorkoutTracker.Core.Entities;

public class ExerciseSet
{
    public int Id { get; set; }
    public int WorkoutId { get; set; }
    public int SetNumber { get; set; }
    public int Reps { get; set; }
    public decimal? Weight { get; set; }
    public bool IsWarmup { get; set; }
    public string? Notes { get; set; }
    public Workout Workout { get; set; } = null!;
}
```

**WorkoutTracker.Core/Interfaces/IWorkoutRepository.cs**
```csharp
namespace WorkoutTracker.Core.Interfaces;

using WorkoutTracker.Core.Entities;

public interface IWorkoutRepository
{
    Task<Workout?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Workout>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Workout>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<Workout>> GetByExerciseNameAsync(string exerciseName, CancellationToken cancellationToken = default);
    Task<Workout> AddAsync(Workout workout, CancellationToken cancellationToken = default);
    Task UpdateAsync(Workout workout, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
```

### Step 4: Build Infrastructure Layer

**WorkoutTracker.Infrastructure/Data/WorkoutDbContext.cs**
```csharp
namespace WorkoutTracker.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Core.Entities;

public class WorkoutDbContext : DbContext
{
    public WorkoutDbContext(DbContextOptions<WorkoutDbContext> options) : base(options)
    {
    }

    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<ExerciseSet> ExerciseSets => Set<ExerciseSet>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkoutDbContext).Assembly);
    }
}
```

**WorkoutTracker.Infrastructure/Data/Configurations/WorkoutConfiguration.cs**
```csharp
namespace WorkoutTracker.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutTracker.Core.Entities;

public class WorkoutConfiguration : IEntityTypeConfiguration<Workout>
{
    public void Configure(EntityTypeBuilder<Workout> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.ExerciseName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.Category)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(w => w.Weight)
            .HasPrecision(8, 2);

        builder.Property(w => w.WeightUnit)
            .HasMaxLength(10);

        builder.Property(w => w.Notes)
            .HasMaxLength(500);

        builder.HasIndex(w => w.PerformedAt)
            .HasDatabaseName("IX_Workouts_PerformedAt");

        builder.HasIndex(w => new { w.ExerciseName, w.PerformedAt })
            .HasDatabaseName("IX_Workouts_ExerciseName_PerformedAt");

        builder.Property(w => w.CreatedAt)
            .HasDefaultValueSql("datetime('now')");

        builder.HasMany(w => w.ExerciseSets)
            .WithOne(es => es.Workout)
            .HasForeignKey(es => es.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

**WorkoutTracker.Infrastructure/Data/Configurations/ExerciseSetConfiguration.cs**
```csharp
namespace WorkoutTracker.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutTracker.Core.Entities;

public class ExerciseSetConfiguration : IEntityTypeConfiguration<ExerciseSet>
{
    public void Configure(EntityTypeBuilder<ExerciseSet> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Weight)
            .HasPrecision(8, 2);

        builder.Property(e => e.Notes)
            .HasMaxLength(200);
    }
}
```

**WorkoutTracker.Infrastructure/Repositories/WorkoutRepository.cs**
```csharp
namespace WorkoutTracker.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Core.Entities;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Infrastructure.Data;

public class WorkoutRepository : IWorkoutRepository
{
    private readonly WorkoutDbContext _context;

    public WorkoutRepository(WorkoutDbContext context)
    {
        _context = context;
    }

    public async Task<Workout?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Workouts
            .Include(w => w.ExerciseSets)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Workout>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Workouts
            .Include(w => w.ExerciseSets)
            .OrderByDescending(w => w.PerformedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Workout>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.Workouts
            .Where(w => w.PerformedAt >= startDate && w.PerformedAt <= endDate)
            .OrderByDescending(w => w.PerformedAt)
            .Include(w => w.ExerciseSets)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Workout>> GetByExerciseNameAsync(
        string exerciseName,
        CancellationToken cancellationToken = default)
    {
        return await _context.Workouts
            .Where(w => w.ExerciseName.ToLower().Contains(exerciseName.ToLower()))
            .OrderByDescending(w => w.PerformedAt)
            .Include(w => w.ExerciseSets)
            .ToListAsync(cancellationToken);
    }

    public async Task<Workout> AddAsync(Workout workout, CancellationToken cancellationToken = default)
    {
        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync(cancellationToken);
        return workout;
    }

    public async Task UpdateAsync(Workout workout, CancellationToken cancellationToken = default)
    {
        workout.UpdatedAt = DateTime.UtcNow;
        _context.Workouts.Update(workout);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var workout = await _context.Workouts.FindAsync(new object[] { id }, cancellationToken);
        if (workout != null)
        {
            _context.Workouts.Remove(workout);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Workouts.AnyAsync(w => w.Id == id, cancellationToken);
    }
}
```

### Step 5: Build Application Layer

**WorkoutTracker.Application/DTOs/CreateWorkoutRequest.cs**
```csharp
namespace WorkoutTracker.Application.DTOs;

using System.ComponentModel.DataAnnotations;
using WorkoutTracker.Core.Enums;

public record CreateWorkoutRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string ExerciseName { get; init; } = string.Empty;

    [Required]
    public ExerciseCategory Category { get; init; }

    [Range(1, 100)]
    public int Sets { get; init; }

    [Range(1, 1000)]
    public int Reps { get; init; }

    [Range(0, 2000)]
    public decimal? Weight { get; init; }

    [RegularExpression("^(kg|lbs)$", ErrorMessage = "WeightUnit must be 'kg' or 'lbs'")]
    public string? WeightUnit { get; init; }

    public DateTime? PerformedAt { get; init; }

    public TimeSpan? Duration { get; init; }

    [StringLength(500)]
    public string? Notes { get; init; }

    [Range(1, 10)]
    public int? PerceivedExertion { get; init; }

    public List<CreateExerciseSetRequest>? ExerciseSets { get; init; }
}

public record CreateExerciseSetRequest
{
    [Range(1, 100)]
    public int SetNumber { get; init; }

    [Range(1, 1000)]
    public int Reps { get; init; }

    [Range(0, 2000)]
    public decimal? Weight { get; init; }

    public bool IsWarmup { get; init; }

    [StringLength(200)]
    public string? Notes { get; init; }
}
```

**WorkoutTracker.Application/DTOs/UpdateWorkoutRequest.cs**
```csharp
namespace WorkoutTracker.Application.DTOs;

using System.ComponentModel.DataAnnotations;
using WorkoutTracker.Core.Enums;

public record UpdateWorkoutRequest
{
    [StringLength(100, MinimumLength = 1)]
    public string? ExerciseName { get; init; }

    public ExerciseCategory? Category { get; init; }

    [Range(1, 100)]
    public int? Sets { get; init; }

    [Range(1, 1000)]
    public int? Reps { get; init; }

    [Range(0, 2000)]
    public decimal? Weight { get; init; }

    [RegularExpression("^(kg|lbs)$", ErrorMessage = "WeightUnit must be 'kg' or 'lbs'")]
    public string? WeightUnit { get; init; }

    public DateTime? PerformedAt { get; init; }

    public TimeSpan? Duration { get; init; }

    [StringLength(500)]
    public string? Notes { get; init; }

    [Range(1, 10)]
    public int? PerceivedExertion { get; init; }
}
```

**WorkoutTracker.Application/DTOs/WorkoutResponse.cs**
```csharp
namespace WorkoutTracker.Application.DTOs;

using WorkoutTracker.Core.Enums;

public record WorkoutResponse
{
    public int Id { get; init; }
    public string ExerciseName { get; init; } = string.Empty;
    public ExerciseCategory Category { get; init; }
    public int Sets { get; init; }
    public int Reps { get; init; }
    public decimal? Weight { get; init; }
    public string? WeightUnit { get; init; }
    public DateTime PerformedAt { get; init; }
    public TimeSpan? Duration { get; init; }
    public string? Notes { get; init; }
    public int? PerceivedExertion { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public List<ExerciseSetResponse> ExerciseSets { get; init; } = new();
}

public record ExerciseSetResponse
{
    public int Id { get; init; }
    public int SetNumber { get; init; }
    public int Reps { get; init; }
    public decimal? Weight { get; init; }
    public bool IsWarmup { get; init; }
    public string? Notes { get; init; }
}
```

**WorkoutTracker.Application/Services/IWorkoutService.cs**
```csharp
namespace WorkoutTracker.Application.Services;

using WorkoutTracker.Application.DTOs;

public interface IWorkoutService
{
    Task<WorkoutResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutResponse>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutResponse>> GetByExerciseNameAsync(string exerciseName, CancellationToken cancellationToken = default);
    Task<WorkoutResponse> CreateAsync(CreateWorkoutRequest request, CancellationToken cancellationToken = default);
    Task<WorkoutResponse> UpdateAsync(int id, UpdateWorkoutRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
```

**WorkoutTracker.Application/Services/WorkoutService.cs**
```csharp
namespace WorkoutTracker.Application.Services;

using WorkoutTracker.Application.DTOs;
using WorkoutTracker.Core.Entities;
using WorkoutTracker.Core.Interfaces;

public class WorkoutService : IWorkoutService
{
    private readonly IWorkoutRepository _repository;

    public WorkoutService(IWorkoutRepository repository)
    {
        _repository = repository;
    }

    public async Task<WorkoutResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var workout = await _repository.GetByIdAsync(id, cancellationToken);
        return workout is null ? null : MapToResponse(workout);
    }

    public async Task<IEnumerable<WorkoutResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var workouts = await _repository.GetAllAsync(cancellationToken);
        return workouts.Select(MapToResponse);
    }

    public async Task<IEnumerable<WorkoutResponse>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var workouts = await _repository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        return workouts.Select(MapToResponse);
    }

    public async Task<IEnumerable<WorkoutResponse>> GetByExerciseNameAsync(
        string exerciseName,
        CancellationToken cancellationToken = default)
    {
        var workouts = await _repository.GetByExerciseNameAsync(exerciseName, cancellationToken);
        return workouts.Select(MapToResponse);
    }

    public async Task<WorkoutResponse> CreateAsync(CreateWorkoutRequest request, CancellationToken cancellationToken = default)
    {
        var workout = new Workout
        {
            ExerciseName = request.ExerciseName,
            Category = request.Category,
            Sets = request.Sets,
            Reps = request.Reps,
            Weight = request.Weight,
            WeightUnit = request.WeightUnit,
            PerformedAt = request.PerformedAt ?? DateTime.UtcNow,
            Duration = request.Duration,
            Notes = request.Notes,
            PerceivedExertion = request.PerceivedExertion,
            CreatedAt = DateTime.UtcNow,
            ExerciseSets = request.ExerciseSets?.Select(s => new ExerciseSet
            {
                SetNumber = s.SetNumber,
                Reps = s.Reps,
                Weight = s.Weight,
                IsWarmup = s.IsWarmup,
                Notes = s.Notes
            }).ToList() ?? new List<ExerciseSet>()
        };

        var created = await _repository.AddAsync(workout, cancellationToken);
        return MapToResponse(created);
    }

    public async Task<WorkoutResponse> UpdateAsync(int id, UpdateWorkoutRequest request, CancellationToken cancellationToken = default)
    {
        var workout = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Workout with ID {id} not found");

        if (request.ExerciseName is not null) workout.ExerciseName = request.ExerciseName;
        if (request.Category.HasValue) workout.Category = request.Category.Value;
        if (request.Sets.HasValue) workout.Sets = request.Sets.Value;
        if (request.Reps.HasValue) workout.Reps = request.Reps.Value;
        if (request.Weight.HasValue) workout.Weight = request.Weight.Value;
        if (request.WeightUnit is not null) workout.WeightUnit = request.WeightUnit;
        if (request.PerformedAt.HasValue) workout.PerformedAt = request.PerformedAt.Value;
        if (request.Duration.HasValue) workout.Duration = request.Duration.Value;
        if (request.Notes is not null) workout.Notes = request.Notes;
        if (request.PerceivedExertion.HasValue) workout.PerceivedExertion = request.PerceivedExertion.Value;

        await _repository.UpdateAsync(workout, cancellationToken);
        return MapToResponse(workout);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.ExistsAsync(id, cancellationToken);
    }

    private static WorkoutResponse MapToResponse(Workout workout)
    {
        return new WorkoutResponse
        {
            Id = workout.Id,
            ExerciseName = workout.ExerciseName,
            Category = workout.Category,
            Sets = workout.Sets,
            Reps = workout.Reps,
            Weight = workout.Weight,
            WeightUnit = workout.WeightUnit,
            PerformedAt = workout.PerformedAt,
            Duration = workout.Duration,
            Notes = workout.Notes,
            PerceivedExertion = workout.PerceivedExertion,
            CreatedAt = workout.CreatedAt,
            UpdatedAt = workout.UpdatedAt,
            ExerciseSets = workout.ExerciseSets.Select(s => new ExerciseSetResponse
            {
                Id = s.Id,
                SetNumber = s.SetNumber,
                Reps = s.Reps,
                Weight = s.Weight,
                IsWarmup = s.IsWarmup,
                Notes = s.Notes
            }).ToList()
        };
    }
}
```

### Step 6: Build API Layer

**WorkoutTracker.Api/Middleware/RequestTimingMiddleware.cs**
```csharp
namespace WorkoutTracker.Api.Middleware;

using System.Diagnostics;

public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        var correlationId = Guid.NewGuid().ToString();
        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers.Append("X-Correlation-Id", correlationId);

        _logger.LogInformation(
            "[{CorrelationId}] Request STARTED: {Method} {Path}",
            correlationId, context.Request.Method, context.Request.Path);

        await _next(context);

        stopwatch.Stop();

        _logger.LogInformation(
            "[{CorrelationId}] Request COMPLETED in {ElapsedMs}ms with status {StatusCode}",
            correlationId, stopwatch.ElapsedMilliseconds, context.Response.StatusCode);

        context.Response.Headers.Append("X-Response-Time-Ms", stopwatch.ElapsedMilliseconds.ToString());
    }
}
```

**WorkoutTracker.Api/Middleware/ExceptionHandlingMiddleware.cs**
```csharp
namespace WorkoutTracker.Api.Middleware;

using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

        _logger.LogError(exception,
            "[{CorrelationId}] Unhandled exception occurred: {Message}",
            correlationId, exception.Message);

        var statusCode = exception switch
        {
            ArgumentException => HttpStatusCode.BadRequest,
            KeyNotFoundException => HttpStatusCode.NotFound,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            _ => HttpStatusCode.InternalServerError
        };

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = GetTitle(statusCode),
            Detail = _environment.IsDevelopment() ? exception.Message : "An error occurred processing your request.",
            Instance = context.Request.Path
        };

        problemDetails.Extensions["correlationId"] = correlationId;
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private static string GetTitle(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.BadRequest => "Bad Request",
        HttpStatusCode.NotFound => "Not Found",
        HttpStatusCode.Unauthorized => "Unauthorized",
        HttpStatusCode.InternalServerError => "Internal Server Error",
        _ => "Error"
    };
}
```

**WorkoutTracker.Api/Middleware/RequestLoggingMiddleware.cs**
```csharp
namespace WorkoutTracker.Api.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "N/A";

        _logger.LogDebug(
            "[{CorrelationId}] Request Details: " +
            "Scheme={Scheme}, Host={Host}, Path={Path}, QueryString={QueryString}, " +
            "ContentType={ContentType}, ContentLength={ContentLength}",
            correlationId,
            context.Request.Scheme,
            context.Request.Host,
            context.Request.Path,
            context.Request.QueryString,
            context.Request.ContentType,
            context.Request.ContentLength);

        foreach (var header in context.Request.Headers
            .Where(h => !h.Key.Contains("Authorization", StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogTrace("[{CorrelationId}] Header: {Key}={Value}",
                correlationId, header.Key, header.Value);
        }

        await _next(context);
    }
}
```

**WorkoutTracker.Api/Filters/ActionLoggingFilter.cs**
```csharp
namespace WorkoutTracker.Api.Filters;

using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

public class ActionLoggingFilter : IAsyncActionFilter
{
    private readonly ILogger<ActionLoggingFilter> _logger;

    public ActionLoggingFilter(ILogger<ActionLoggingFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var correlationId = context.HttpContext.Items["CorrelationId"]?.ToString() ?? "N/A";
        var actionName = context.ActionDescriptor.DisplayName;

        _logger.LogInformation(
            "[{CorrelationId}] ACTION START: {ActionName}",
            correlationId, actionName);

        foreach (var arg in context.ActionArguments)
        {
            var serialized = JsonSerializer.Serialize(arg.Value, new JsonSerializerOptions
            {
                WriteIndented = false,
                MaxDepth = 3
            });
            _logger.LogDebug(
                "[{CorrelationId}] Argument [{Name}]: {Value}",
                correlationId, arg.Key, serialized);
        }

        var resultContext = await next();

        if (resultContext.Exception != null)
        {
            _logger.LogWarning(
                "[{CorrelationId}] ACTION FAILED: {ActionName} with exception {ExceptionType}",
                correlationId, actionName, resultContext.Exception.GetType().Name);
        }
        else
        {
            _logger.LogInformation(
                "[{CorrelationId}] ACTION COMPLETE: {ActionName}",
                correlationId, actionName);
        }
    }
}
```

**WorkoutTracker.Api/Controllers/WorkoutsController.cs**
```csharp
namespace WorkoutTracker.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.Application.DTOs;
using WorkoutTracker.Application.Services;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class WorkoutsController : ControllerBase
{
    private readonly IWorkoutService _workoutService;
    private readonly ILogger<WorkoutsController> _logger;

    public WorkoutsController(IWorkoutService workoutService, ILogger<WorkoutsController> logger)
    {
        _workoutService = workoutService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all workouts, optionally filtered by date range
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WorkoutResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WorkoutResponse>>> GetAll(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all workouts. StartDate: {StartDate}, EndDate: {EndDate}",
            startDate, endDate);

        IEnumerable<WorkoutResponse> workouts;

        if (startDate.HasValue && endDate.HasValue)
        {
            workouts = await _workoutService.GetByDateRangeAsync(
                startDate.Value, endDate.Value, cancellationToken);
        }
        else
        {
            workouts = await _workoutService.GetAllAsync(cancellationToken);
        }

        return Ok(workouts);
    }

    /// <summary>
    /// Gets a specific workout by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WorkoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkoutResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting workout with ID: {Id}", id);

        var workout = await _workoutService.GetByIdAsync(id, cancellationToken);

        if (workout is null)
        {
            return NotFound(new { message = $"Workout with ID {id} not found" });
        }

        return Ok(workout);
    }

    /// <summary>
    /// Creates a new workout entry
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(WorkoutResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkoutResponse>> Create(
        [FromBody] CreateWorkoutRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new workout for exercise: {ExerciseName}",
            request.ExerciseName);

        var created = await _workoutService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created);
    }

    /// <summary>
    /// Updates an existing workout
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(WorkoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkoutResponse>> Update(
        int id,
        [FromBody] UpdateWorkoutRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating workout with ID: {Id}", id);

        if (!await _workoutService.ExistsAsync(id, cancellationToken))
        {
            return NotFound(new { message = $"Workout with ID {id} not found" });
        }

        var updated = await _workoutService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a workout
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting workout with ID: {Id}", id);

        if (!await _workoutService.ExistsAsync(id, cancellationToken))
        {
            return NotFound(new { message = $"Workout with ID {id} not found" });
        }

        await _workoutService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Search workouts by exercise name
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<WorkoutResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WorkoutResponse>>> Search(
        [FromQuery] string exerciseName,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Searching workouts for exercise: {ExerciseName}", exerciseName);

        var workouts = await _workoutService.GetByExerciseNameAsync(
            exerciseName, cancellationToken);

        return Ok(workouts);
    }
}
```

**WorkoutTracker.Api/Program.cs**
```csharp
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Api.Filters;
using WorkoutTracker.Api.Middleware;
using WorkoutTracker.Application.Services;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Infrastructure.Data;
using WorkoutTracker.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// =============================================================
// SERVICE REGISTRATION (Dependency Injection Container)
// =============================================================

// Database Context
builder.Services.AddDbContext<WorkoutDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("WorkoutTracker.Infrastructure")));

// Repository Layer
builder.Services.AddScoped<IWorkoutRepository, WorkoutRepository>();

// Application Services
builder.Services.AddScoped<IWorkoutService, WorkoutService>();

// Controllers with filters
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ActionLoggingFilter>();
});

// API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Workout Tracker API",
        Version = "v1",
        Description = "A portfolio API demonstrating the ASP.NET Core request lifecycle"
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// =============================================================
// MIDDLEWARE PIPELINE (Order matters!)
// =============================================================

// 1. Request Timing - FIRST to capture total request time
app.UseMiddleware<RequestTimingMiddleware>();

// 2. Exception Handling - Wraps everything below in try/catch
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 3. Request Logging - Log details of each request
app.UseMiddleware<RequestLoggingMiddleware>();

// 4. Development tools
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 5. HTTPS Redirection
app.UseHttpsRedirection();

// 6. CORS
app.UseCors();

// 7. Routing - Matches URL to endpoint
app.UseRouting();

// 8. Map Controllers - Endpoint execution
app.MapControllers();

// =============================================================
// DATABASE INITIALIZATION
// =============================================================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WorkoutDbContext>();
    db.Database.EnsureCreated();
}

app.Run();

// Make the implicit Program class public for integration testing
public partial class Program { }
```

**WorkoutTracker.Api/appsettings.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=workouts.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "WorkoutTracker": "Debug"
    }
  },
  "AllowedHosts": "*"
}
```

**WorkoutTracker.Api/appsettings.Development.json**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information",
      "WorkoutTracker": "Trace"
    }
  }
}
```

### Step 7: Create Database

```bash
cd WorkoutTracker.Api
dotnet ef migrations add InitialCreate --project ../WorkoutTracker.Infrastructure
dotnet ef database update --project ../WorkoutTracker.Infrastructure
```

### Step 8: Add Tests (Optional)

**WorkoutTracker.Tests/Unit/Services/WorkoutServiceTests.cs**
```csharp
namespace WorkoutTracker.Tests.Unit.Services;

using Moq;
using WorkoutTracker.Application.DTOs;
using WorkoutTracker.Application.Services;
using WorkoutTracker.Core.Entities;
using WorkoutTracker.Core.Enums;
using WorkoutTracker.Core.Interfaces;

public class WorkoutServiceTests
{
    private readonly Mock<IWorkoutRepository> _repositoryMock;
    private readonly WorkoutService _service;

    public WorkoutServiceTests()
    {
        _repositoryMock = new Mock<IWorkoutRepository>();
        _service = new WorkoutService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenWorkoutExists_ReturnsWorkoutResponse()
    {
        // Arrange
        var workout = new Workout
        {
            Id = 1,
            ExerciseName = "Bench Press",
            Category = ExerciseCategory.Strength,
            Sets = 3,
            Reps = 10,
            Weight = 135,
            WeightUnit = "lbs",
            PerformedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workout);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Bench Press", result.ExerciseName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenWorkoutDoesNotExist_ReturnsNull()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Workout?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedWorkout()
    {
        // Arrange
        var request = new CreateWorkoutRequest
        {
            ExerciseName = "Squat",
            Category = ExerciseCategory.Strength,
            Sets = 5,
            Reps = 5,
            Weight = 225,
            WeightUnit = "lbs"
        };

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Workout>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Workout w, CancellationToken _) =>
            {
                w.Id = 1;
                return w;
            });

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Squat", result.ExerciseName);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Workout>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

**WorkoutTracker.Tests/Integration/Controllers/WorkoutsControllerTests.cs**
```csharp
namespace WorkoutTracker.Tests.Integration.Controllers;

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkoutTracker.Application.DTOs;
using WorkoutTracker.Core.Enums;
using WorkoutTracker.Infrastructure.Data;

public class WorkoutsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public WorkoutsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<WorkoutDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<WorkoutDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid()));
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithEmptyList_WhenNoWorkoutsExist()
    {
        var response = await _client.GetAsync("/api/workouts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var workouts = await response.Content.ReadFromJsonAsync<List<WorkoutResponse>>();
        Assert.NotNull(workouts);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WithValidRequest()
    {
        var request = new CreateWorkoutRequest
        {
            ExerciseName = "Deadlift",
            Category = ExerciseCategory.Strength,
            Sets = 5,
            Reps = 5,
            Weight = 315,
            WeightUnit = "lbs"
        };

        var response = await _client.PostAsJsonAsync("/api/workouts", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<WorkoutResponse>();
        Assert.NotNull(created);
        Assert.Equal("Deadlift", created.ExerciseName);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenWorkoutDoesNotExist()
    {
        var response = await _client.GetAsync("/api/workouts/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Response_ContainsCorrelationId_Header()
    {
        var response = await _client.GetAsync("/api/workouts");

        Assert.True(response.Headers.Contains("X-Correlation-Id"));
    }
}
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/workouts | Get all workouts (optional ?startDate & ?endDate) |
| GET | /api/workouts/{id} | Get workout by ID |
| POST | /api/workouts | Create new workout |
| PUT | /api/workouts/{id} | Update existing workout |
| DELETE | /api/workouts/{id} | Delete workout |
| GET | /api/workouts/search?exerciseName= | Search by exercise name |

## Verification

1. **Run the API**:
   ```bash
   cd WorkoutTracker.Api
   dotnet run
   ```

2. **Test with Swagger**: Navigate to `https://localhost:5001/swagger` (or the port shown in console)

3. **Verify request lifecycle** - Check console logs for:
   - Correlation ID assignment
   - Request timing
   - Action filter logging
   - Response time headers (X-Correlation-Id, X-Response-Time-Ms)

4. **Run tests**:
   ```bash
   dotnet test
   ```

## Portfolio Highlights

This project demonstrates:
- **Clean Architecture** with proper separation of concerns
- **Complete ASP.NET Core request pipeline** understanding via custom middleware
- **Observability** - Correlation IDs, structured logging, timing headers
- **RFC 7807 Problem Details** error handling
- **Repository pattern** with Entity Framework Core
- **Time-series data modeling** with proper indexing
- **Unit and integration testing** with mocking and WebApplicationFactory
