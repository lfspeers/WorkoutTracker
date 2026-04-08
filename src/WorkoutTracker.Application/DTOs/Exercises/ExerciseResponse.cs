namespace WorkoutTracker.Application.DTOs.Exercises {
    public record ExerciseResponse {
        public int Id {get; init;}
        public string Name {get; init;}
        public string Category {get; init;}
        public string? MuscleGroup {get; init;}
        public string? Description {get; init;}
        public DateTime CreatedAt {get; init;}
    }
}