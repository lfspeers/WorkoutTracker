using System.ComponentModel.DataAnnotations;

namespace WorkoutTracker.Application.DTOs.WorkoutSessions {
    public class CreateWorkoutExerciseRequest {
        [Required] public int ExerciseId {get; set;}
        [Required] public int Order {get; set;}
        [RegularExpression("^(kg|lbs)$")] public string? WeightUnit {get; set;}
        [MaxLength(200)] public string? Notes {get; set;}
        [Required] public List<CreateSetRequest> Sets {get; set;}
    }
}