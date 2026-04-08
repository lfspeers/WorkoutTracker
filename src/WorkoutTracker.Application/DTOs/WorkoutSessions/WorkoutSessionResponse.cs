namespace WorkoutTracker.Application.DTOs.WorkoutSessions {
    public record SetResponse {
        public int Id {get; init;}
        public int SetNumber {get; init;}
        public int Reps {get; init;}
        public decimal? Weight {get; init;}
        public bool IsWarmup {get; init;}
        public string? Notes {get; init;}
    }

    public record WorkoutExerciseResponse {
        public int Id {get; init;}
        public int ExerciseId {get; init;}
        public string ExerciseName {get; init;}
        public string ExerciseCategory {get; init;}
        public int Order {get; init;}
        public string? WeightUnit {get; init;}
        public string? Notes {get; init;}
        public List<SetResponse> Sets {get; init;}
    }

    public record WorkoutSessionResponse {
        public int Id {get; init;}
        public DateTime PerformedAt {get; init;}
        public TimeSpan? Duration {get; init;}
        public string? Notes {get; init;}
        public int? PerceivedExertion {get; init;}
        public List<WorkoutExerciseResponse>? Exercises {get; init;}
        public DateTime CreatedAt {get; init;}
        public DateTime? UpdatedAt {get; init;}
    }
}