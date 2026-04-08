using System.ComponentModel.DataAnnotations;
using WorkoutTracker.Core.Enums;

namespace WorkoutTracker.Application.DTOs.Exercises {
    public class UpdateExerciseRequest {
        [MaxLength(100)] public string? Name {get; set;}
        public ExerciseCategory? Category {get; set;}
        [MaxLength(50)] public string? MuscleGroup {get; set;}
        [MaxLength(500)] public string? Description {get; set;}
    }
}