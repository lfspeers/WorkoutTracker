# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Important: Collaboration Mode

The user is learning C# and building this app to develop their skills. **Do not write code directly.** Instead, guide and instruct — explain what to do, why, and let the user implement it themselves. Refer to PLAN.md for the implementation roadmap and BACKLOG.md for task tracking.

## Build & Run Commands

```bash
dotnet build                    # Build entire solution
dotnet test                     # Run all tests
dotnet test --filter "FullyQualifiedName~TestMethodName"  # Run a single test
cd src/WorkoutTracker.Api && dotnet run                   # Run the API (http://localhost:5137)
dotnet ef migrations add <Name> --project src/WorkoutTracker.Infrastructure --startup-project src/WorkoutTracker.Api
dotnet ef database update --project src/WorkoutTracker.Infrastructure --startup-project src/WorkoutTracker.Api
```

## Architecture

Clean Architecture with four layers, enforced via project references:

```
Core (innermost — no dependencies)
  ↑
Application → references Core
Infrastructure → references Core
  ↑
Api → references Application + Infrastructure (composition root)
```

- **Core** (`WorkoutTracker.Core`): Domain entities, enums, repository interfaces. No external dependencies.
- **Application** (`WorkoutTracker.Application`): DTOs, services, validators. Not yet implemented.
- **Infrastructure** (`WorkoutTracker.Infrastructure`): EF Core DbContext, Fluent API entity configurations, repository implementations. Uses SQLite.
- **Api** (`WorkoutTracker.Api`): ASP.NET Core Web API. Currently has boilerplate only — controllers not yet built.
- **Tests** (`WorkoutTracker.Tests`): xUnit + Moq + EF Core InMemory provider.

## Domain Model

Four entities with this relationship chain: **WorkoutSession → WorkoutExercise → Set**, with **Exercise** as reference data linked through WorkoutExercise.

- `WorkoutSession` has many `WorkoutExercise` (cascade delete)
- `WorkoutExercise` belongs to both `WorkoutSession` (cascade) and `Exercise` (restrict delete)
- `WorkoutExercise` has many `Set` (cascade delete)
- `Exercise` uses `ExerciseCategory` enum (stored as string via Fluent API value conversion)

## Key Patterns

- **Repository pattern**: Interfaces in Core (`IExerciseRepository`, `IWorkoutSessionRepository`), implementations in Infrastructure. All methods are async with `CancellationToken` support.
- **EF Core Fluent API**: All entity configuration is in `Infrastructure/Data/Configurations/` using `IEntityTypeConfiguration<T>` — no data annotations on entities.
- **Eager loading**: `WorkoutSessionRepository` uses `.Include().ThenInclude()` chains to load the full WorkoutSession → WorkoutExercise → Set/Exercise graph.

## Tech Stack

- .NET 10.0, ASP.NET Core, EF Core with SQLite
- Solution file: `WorkoutTracker.slnx` (XML format)
