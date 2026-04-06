using WorkoutTracker.Core.Entities;

namespace WorkoutTracker.Core.Interfaces
{
    public interface IWorkoutSessionRepository
    {
        Task<WorkoutSession?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkoutSession>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkoutSession>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default);
        Task<WorkoutSession> AddAsync(WorkoutSession workoutSession, CancellationToken cancellationToken = default);
        Task<WorkoutSession> UpdateAsync(WorkoutSession workoutSession, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
        
    }
}