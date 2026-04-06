namespace WorkoutTracker.Core.Entities
{
    public class Set
    {
        public int Id {get; set;}
        public int WorkoutExerciseId {get; set;}
        public int SetNumber {get; set;}
        public int Reps {get; set;}
        public decimal? Weight {get; set;}
        public bool IsWarmup {get; set;}
        public string? Notes {get; set;}
        public WorkoutExercise WorkoutExercise {get; set;}
    }
}