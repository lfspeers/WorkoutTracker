namespace WorkoutTracker.Core.Entities
{
    public class WorkoutSession
    {
        public int Id {get; set;}
        public DateTime PerformedAt {get; set;}
        public TimeSpan? Duration {get; set;}
        public string? Notes {get; set;}
        public int? PerceivedExertion {get; set;}
        public DateTime CreatedAt {get; set;}
        public DateTime? UpdatedAt {get; set;}

        public ICollection<WorkoutExercise> WorkoutExercises {get; set;}
    }
}