using System.ComponentModel.DataAnnotations;

namespace WorkoutTracker.Application.DTOs.WorkoutSessions {
    public class CreateSetRequest {
        [Required] public int SetNumber {get; set;}
        [Required] public int Reps {get; set;}
        public decimal? Weight {get; set;}
        public bool IsWarmup {get; set;}
        [MaxLength(200)] public string? Notes {get; set;}
    }
}