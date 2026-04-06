using WorkoutTracker.Core.Entities;
using WorkoutTracker.Core.Enums;

namespace WorkoutTracker.Core.Interfaces
{
    public interface IExerciseRepository
    {
        Task<Exercise?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Exercise>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Exercise>> GetByCategoryAsync(ExerciseCategory exerciseCategory, CancellationToken cancellationToken = default);
        Task<Exercise> AddAsync(Exercise exercise, CancellationToken cancellationToken = default);
        Task<Exercise> UpdateAsync(Exercise exercise, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
        
    }
}