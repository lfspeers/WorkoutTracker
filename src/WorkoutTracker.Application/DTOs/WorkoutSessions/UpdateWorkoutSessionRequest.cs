using System.ComponentModel.DataAnnotations;

namespace WorkoutTracker.Application.DTOs.WorkoutSessions {
    public class UpdateWorkoutSessionRequest {
        public DateTime? PerformedAt {get; set;}
        public TimeSpan? Duration {get; set;}
        [MaxLength(500)] public string? Notes {get; set;}
        [Range(1, 10)] public int? PerceivedExertion {get; set;}
    }
}