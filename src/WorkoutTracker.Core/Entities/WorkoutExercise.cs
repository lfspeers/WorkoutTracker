namespace WorkoutTracker.Core.Entities
{
    public class WorkoutExercise
    {
        public int Id {get; set;}
        public int WorkoutSessionId {get; set;}
        public int ExerciseId {get; set;}
        public int Order {get; set;}
        public string? WeightUnit {get; set;}
        public string? Notes {get; set;}
        public WorkoutSession WorkoutSession {get; set;}
        public Exercise Exercise {get; set;}
        public ICollection<Set> Sets {get; set;}
    }
}