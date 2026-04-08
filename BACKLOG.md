# Workout Tracker API - Backlog

## Overview
| Metric | Count |
|--------|-------|
| Backlog Stories | 9 |
| Estimated Tasks | 26 |
| Completed Stories | 5 |
| Next Up | Story 3.3: Implement Repositories |

---

## Data Model (Fully Normalized)

```
┌─────────────────┐       ┌─────────────────────┐       ┌─────────────────┐
│    Exercise     │       │   WorkoutSession    │       │      Set        │
├─────────────────┤       ├─────────────────────┤       ├─────────────────┤
│ Id (PK)         │       │ Id (PK)             │       │ Id (PK)         │
│ Name            │◄──┐   │ PerformedAt         │       │ WorkoutExercise │
│ Category        │   │   │ Duration            │       │   Id (FK)       │
│ MuscleGroup     │   │   │ Notes               │       │ SetNumber       │
│ Description     │   │   │ PerceivedExertion   │       │ Reps            │
│ CreatedAt       │   │   │ CreatedAt           │       │ Weight          │
└─────────────────┘   │   │ UpdatedAt           │       │ IsWarmup        │
                      │   └──────────┬──────────┘       │ Notes           │
                      │              │ 1                └────────┬────────┘
                      │              ▼ Many                      │
                      │   ┌─────────────────────┐               │
                      │   │  WorkoutExercise    │               │
                      │   ├─────────────────────┤               │
                      └───┤ Id (PK)             │◄──────────────┘
                          │ WorkoutSessionId(FK)│        Many
                          │ ExerciseId (FK)     │
                          │ Order               │
                          │ Notes               │
                          │ WeightUnit          │
                          └─────────────────────┘
```

### Entity Details

| Entity | Field | Type | Nullable | Purpose |
|--------|-------|------|----------|---------|
| **Exercise** | Id | int | | PK |
| | Name | string(100) | | "Bench Press", "Running" |
| | Category | enum | | Strength, Cardio, Flexibility, Balance, Plyometric |
| | MuscleGroup | string(50) | ✓ | "Chest", "Legs", "Full Body" |
| | Description | string(500) | ✓ | Form instructions |
| | CreatedAt | DateTime | | Audit |
| **WorkoutSession** | Id | int | | PK |
| | PerformedAt | DateTime | | When the session occurred |
| | Duration | TimeSpan | ✓ | Total session length |
| | Notes | string(500) | ✓ | "Felt strong today" |
| | PerceivedExertion | int(1-10) | ✓ | Overall session RPE |
| | CreatedAt | DateTime | | Audit |
| | UpdatedAt | DateTime | ✓ | Audit |
| **WorkoutExercise** | Id | int | | PK |
| | WorkoutSessionId | int | | FK → WorkoutSession |
| | ExerciseId | int | | FK → Exercise |
| | Order | int | | 1st, 2nd, 3rd exercise |
| | WeightUnit | string(3) | ✓ | "kg" or "lbs" |
| | Notes | string(200) | ✓ | Exercise-specific notes |
| **Set** | Id | int | | PK |
| | WorkoutExerciseId | int | | FK → WorkoutExercise |
| | SetNumber | int | | 1, 2, 3... |
| | Reps | int | | Reps completed |
| | Weight | decimal(8,2) | ✓ | Weight used |
| | IsWarmup | bool | | Warmup vs working set |
| | Notes | string(200) | ✓ | "Failed on rep 8" |

---

## Epic 1: Project Foundation ✅ COMPLETE

### Story 1.1: Create Solution Structure ✅
**Priority**: CRITICAL | **Est. Tasks**: 3

**As a** developer
**I want** a properly structured .NET solution
**So that** my code is organized following Clean Architecture principles

| Task | Description |
|------|-------------|
| 1.1.1 | Create solution file with `dotnet new sln -n WorkoutTracker` |
| 1.1.2 | Create all 5 projects (Api, Core, Application, Infrastructure, Tests) |
| 1.1.3 | Add all projects to the solution with `dotnet sln add` |

**Acceptance Criteria:**
- [x] Solution file exists at root
- [x] All 5 project folders exist with `.csproj` files
- [x] `dotnet build` succeeds with no errors

---

### Story 1.2: Configure Project References ✅
**Priority**: CRITICAL | **Est. Tasks**: 4

**As a** developer
**I want** proper project dependencies
**So that** each layer can only access what it should

| Task | Description |
|------|-------------|
| 1.2.1 | Api references Application and Infrastructure |
| 1.2.2 | Application references Core |
| 1.2.3 | Infrastructure references Core |
| 1.2.4 | Tests references Api, Application, and Core |

**Acceptance Criteria:**
- [x] Core has NO project references (it's the innermost layer)
- [x] `dotnet build` succeeds
- [x] Dependency diagram matches Clean Architecture

---

### Story 1.3: Install NuGet Packages ✅
**Priority**: CRITICAL | **Est. Tasks**: 3

**As a** developer
**I want** the required NuGet packages installed
**So that** I can use EF Core, Swagger, and testing frameworks

| Task | Description |
|------|-------------|
| 1.3.1 | Infrastructure: Add `Microsoft.EntityFrameworkCore.Sqlite` and `.Design` |
| 1.3.2 | Api: Add `Swashbuckle.AspNetCore` |
| 1.3.3 | Tests: Add `Moq`, `Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.EntityFrameworkCore.InMemory` |

**Acceptance Criteria:**
- [x] All packages restore successfully
- [x] `dotnet build` succeeds

---

## Epic 2: Domain Layer (Core) ✅ COMPLETE
> Moved to FEATURES.md

---

## Epic 3: Infrastructure Layer (Data Access) - IN PROGRESS

### Story 3.1: Create Database Context ✅
> Moved to FEATURES.md

---

### Story 3.2: Create Entity Configurations ✅
> Moved to FEATURES.md

---

### Story 3.3: Implement Repositories ⬅️ NEXT
**Priority**: HIGH | **Est. Tasks**: 2

**As a** developer
**I want** concrete repository implementations
**So that** I can perform CRUD operations

| Task | Description |
|------|-------------|
| 3.3.1 | Create `ExerciseRepository` implementing `IExerciseRepository` |
| 3.3.2 | Create `WorkoutSessionRepository` implementing `IWorkoutSessionRepository` |

**Acceptance Criteria:**
- [ ] All interface methods implemented
- [ ] Uses async/await properly
- [ ] Eager loading for nested entities (WorkoutExercises, Sets)
- [ ] Sets UpdatedAt on updates

---

## Epic 4: Application Layer (Business Logic)

### Story 4.1: Create Exercise DTOs
**Priority**: HIGH | **Est. Tasks**: 3

**As a** developer
**I want** DTOs for the Exercise entity
**So that** I separate my API contracts from domain entities

| Task | Description |
|------|-------------|
| 4.1.1 | Create `CreateExerciseRequest` with validation attributes |
| 4.1.2 | Create `UpdateExerciseRequest` with nullable properties |
| 4.1.3 | Create `ExerciseResponse` record |

**Acceptance Criteria:**
- [ ] CreateExerciseRequest has [Required] Name, [Required] Category
- [ ] Name max length 100, MuscleGroup max 50, Description max 500
- [ ] Response DTOs use `record` type

---

### Story 4.2: Create WorkoutSession DTOs
**Priority**: HIGH | **Est. Tasks**: 3

**As a** developer
**I want** DTOs for WorkoutSession and nested entities
**So that** I can handle complex workout logging requests

| Task | Description |
|------|-------------|
| 4.2.1 | Create `CreateWorkoutSessionRequest` with nested exercise/set DTOs |
| 4.2.2 | Create `UpdateWorkoutSessionRequest` with nullable properties |
| 4.2.3 | Create `WorkoutSessionResponse` with nested response DTOs |

**Acceptance Criteria:**
- [ ] CreateWorkoutSessionRequest supports nested WorkoutExercises with Sets
- [ ] PerceivedExertion range 1-10
- [ ] WeightUnit validated to "kg" or "lbs"
- [ ] Response includes full hierarchy: Session → Exercises → Sets

---

### Story 4.3: Create Service Layer
**Priority**: HIGH | **Est. Tasks**: 2

**As a** developer
**I want** service classes
**So that** business logic is separated from controllers

| Task | Description |
|------|-------------|
| 4.3.1 | Create `IExerciseService` and `ExerciseService` |
| 4.3.2 | Create `IWorkoutSessionService` and `WorkoutSessionService` |

**Acceptance Criteria:**
- [ ] Service methods return DTOs, not entities
- [ ] CreateAsync sets CreatedAt and default PerformedAt
- [ ] UpdateAsync only updates non-null properties
- [ ] Private mapping methods handle entity ↔ DTO conversion

---

## Epic 5: API Layer (Controllers & Middleware)

### Story 5.1: Create Custom Middleware
**Priority**: MEDIUM | **Est. Tasks**: 3

**As a** developer
**I want** custom middleware
**So that** I understand and can demonstrate the request pipeline

| Task | Description |
|------|-------------|
| 5.1.1 | Create `RequestTimingMiddleware` - adds correlation ID, measures duration |
| 5.1.2 | Create `ExceptionHandlingMiddleware` - global try/catch, Problem Details |
| 5.1.3 | Create `RequestLoggingMiddleware` - logs request details |

**Acceptance Criteria:**
- [ ] X-Correlation-Id header added to all responses
- [ ] X-Response-Time-Ms header shows request duration
- [ ] Exceptions return RFC 7807 Problem Details JSON
- [ ] Stack traces only shown in Development environment

---

### Story 5.2: Create Action Filter
**Priority**: MEDIUM | **Est. Tasks**: 1

**As a** developer
**I want** an action filter
**So that** I can log controller action execution

| Task | Description |
|------|-------------|
| 5.2.1 | Create `ActionLoggingFilter` - logs before/after action execution |

**Acceptance Criteria:**
- [ ] Logs action name and correlation ID
- [ ] Logs action arguments (model binding results)
- [ ] Logs success or failure after execution

---

### Story 5.3: Create Controllers
**Priority**: HIGH | **Est. Tasks**: 2

**As a** developer
**I want** REST controllers
**So that** clients can perform CRUD operations via HTTP

| Task | Description |
|------|-------------|
| 5.3.1 | Create `ExercisesController` with CRUD endpoints |
| 5.3.2 | Create `WorkoutSessionsController` with CRUD endpoints |

**Acceptance Criteria:**
- [ ] ExercisesController: GET all, GET by ID, GET by category, POST, PUT, DELETE
- [ ] WorkoutSessionsController: GET all (with date filter), GET by ID, POST, PUT, DELETE
- [ ] Proper HTTP status codes (200, 201, 204, 400, 404)
- [ ] XML documentation comments for Swagger

---

### Story 5.4: Configure Program.cs
**Priority**: CRITICAL | **Est. Tasks**: 2

**As a** developer
**I want** proper startup configuration
**So that** all services are registered and middleware is ordered correctly

| Task | Description |
|------|-------------|
| 5.4.1 | Register all services (DbContext, Repositories, Services) in DI container |
| 5.4.2 | Configure middleware pipeline in correct order |

**Acceptance Criteria:**
- [ ] Middleware order: Timing → Exception → Logging → Swagger → HTTPS → CORS → Routing → Controllers
- [ ] Database auto-created on startup
- [ ] Swagger available in Development
- [ ] `public partial class Program` for integration testing

---

### Story 5.5: Configure App Settings
**Priority**: HIGH | **Est. Tasks**: 2

**As a** developer
**I want** proper configuration files
**So that** connection strings and logging are configured

| Task | Description |
|------|-------------|
| 5.5.1 | Create `appsettings.json` with SQLite connection string |
| 5.5.2 | Create `appsettings.Development.json` with verbose logging |

**Acceptance Criteria:**
- [ ] Connection string points to `workouts.db`
- [ ] Development logging includes EF Core SQL commands
- [ ] WorkoutTracker namespace logs at Debug level

---

## Epic 6: Database Setup

### Story 6.1: Create and Apply Migrations
**Priority**: HIGH | **Est. Tasks**: 2

**As a** developer
**I want** EF Core migrations
**So that** my database schema is version controlled

| Task | Description |
|------|-------------|
| 6.1.1 | Run `dotnet ef migrations add InitialCreate` |
| 6.1.2 | Run `dotnet ef database update` |

**Acceptance Criteria:**
- [ ] Migrations folder created in Infrastructure project
- [ ] `workouts.db` file created in Api project
- [ ] Tables created: Exercises, WorkoutSessions, WorkoutExercises, Sets
- [ ] Indexes created on PerformedAt and Exercise.Name

---

## Epic 7: Testing (Optional but Recommended)

### Story 7.1: Write Unit Tests
**Priority**: LOW | **Est. Tasks**: 2

**As a** developer
**I want** unit tests for my services
**So that** I can verify business logic in isolation

| Task | Description |
|------|-------------|
| 7.1.1 | Create `ExerciseServiceTests` with Moq |
| 7.1.2 | Create `WorkoutSessionServiceTests` with Moq |

**Acceptance Criteria:**
- [ ] Tests use mocked repositories
- [ ] Tests verify correct DTO mapping
- [ ] All tests pass

---

### Story 7.2: Write Integration Tests
**Priority**: LOW | **Est. Tasks**: 2

**As a** developer
**I want** integration tests
**So that** I can verify the full HTTP request/response cycle

| Task | Description |
|------|-------------|
| 7.2.1 | Create `ExercisesControllerTests` using `WebApplicationFactory` |
| 7.2.2 | Create `WorkoutSessionsControllerTests` using `WebApplicationFactory` |

**Acceptance Criteria:**
- [ ] Uses in-memory database
- [ ] Verifies HTTP status codes
- [ ] Verifies X-Correlation-Id header present
- [ ] All tests pass

---

## Recommended Order of Completion

```
1. Epic 1: Project Foundation (Stories 1.1 → 1.2 → 1.3) ✅ COMPLETE
   └── Get the solution structure right first

2. Epic 2: Domain Layer (Stories 2.1 → 2.2) ✅ COMPLETE
   └── Define your core business model (4 entities)

3. Epic 3: Infrastructure Layer (Stories 3.1 → 3.2 → 3.3) ⬅️ IN PROGRESS
   └── Stories 3.1, 3.2 ✅ | Next: Story 3.3 (Repositories)

4. Epic 4: Application Layer (Stories 4.1 → 4.2 → 4.3)
   └── Add business logic and DTOs

5. Epic 6: Database Setup (Story 6.1)
   └── Create the database

6. Epic 5: API Layer (Stories 5.5 → 5.4 → 5.3 → 5.1 → 5.2)
   └── Wire everything together, then add middleware

7. Epic 7: Testing (Stories 7.1 → 7.2)
   └── Verify everything works
```

---

## Quick Reference: File Checklist

### Core Project ✅ COMPLETE
- [x] `Enums/ExerciseCategory.cs`
- [x] `Entities/Exercise.cs`
- [x] `Entities/WorkoutSession.cs`
- [x] `Entities/WorkoutExercise.cs`
- [x] `Entities/Set.cs`
- [x] `Interfaces/IExerciseRepository.cs`
- [x] `Interfaces/IWorkoutSessionRepository.cs`

### Infrastructure Project (IN PROGRESS)
- [x] `Data/WorkoutDbContext.cs`
- [x] `Data/Configurations/ExerciseConfiguration.cs`
- [x] `Data/Configurations/WorkoutSessionConfiguration.cs`
- [x] `Data/Configurations/WorkoutExerciseConfiguration.cs`
- [x] `Data/Configurations/SetConfiguration.cs`
- [ ] `Repositories/ExerciseRepository.cs` ⬅️ NEXT
- [ ] `Repositories/WorkoutSessionRepository.cs`

### Application Project
- [ ] `DTOs/Exercises/CreateExerciseRequest.cs`
- [ ] `DTOs/Exercises/UpdateExerciseRequest.cs`
- [ ] `DTOs/Exercises/ExerciseResponse.cs`
- [ ] `DTOs/WorkoutSessions/CreateWorkoutSessionRequest.cs`
- [ ] `DTOs/WorkoutSessions/UpdateWorkoutSessionRequest.cs`
- [ ] `DTOs/WorkoutSessions/WorkoutSessionResponse.cs`
- [ ] `Services/IExerciseService.cs`
- [ ] `Services/ExerciseService.cs`
- [ ] `Services/IWorkoutSessionService.cs`
- [ ] `Services/WorkoutSessionService.cs`

### Api Project
- [ ] `Middleware/RequestTimingMiddleware.cs`
- [ ] `Middleware/ExceptionHandlingMiddleware.cs`
- [ ] `Middleware/RequestLoggingMiddleware.cs`
- [ ] `Filters/ActionLoggingFilter.cs`
- [ ] `Controllers/ExercisesController.cs`
- [ ] `Controllers/WorkoutSessionsController.cs`
- [ ] `Program.cs` (modify)
- [ ] `appsettings.json` (modify)
- [ ] `appsettings.Development.json` (create)

### Tests Project
- [ ] `Unit/Services/ExerciseServiceTests.cs`
- [ ] `Unit/Services/WorkoutSessionServiceTests.cs`
- [ ] `Integration/Controllers/ExercisesControllerTests.cs`
- [ ] `Integration/Controllers/WorkoutSessionsControllerTests.cs`
