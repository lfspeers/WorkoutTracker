using WorkoutTracker.Core.Enums;

namespace WorkoutTracker.Core.Entities
{
    public class Exercise
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ExerciseCategory Category { get; set; }
        public string? MuscleGroup { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<WorkoutExercise> WorkoutExercises { get; set; }
    }
}