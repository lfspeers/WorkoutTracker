using System.ComponentModel.DataAnnotations;

namespace WorkoutTracker.Application.DTOs.WorkoutSessions {
    public class CreateWorkoutSessionRequest 
    {
        [Required] public DateTime PerformedAt {get; set;}
        public TimeSpan? Duration {get; set;}
        [MaxLength(500)] public string? Notes {get; set;}
        [Range(1, 10)] public int? PerceivedExertion {get; set;}
        [Required] public List<CreateWorkoutExerciseRequest> Exercises {get; set;}
    }
}