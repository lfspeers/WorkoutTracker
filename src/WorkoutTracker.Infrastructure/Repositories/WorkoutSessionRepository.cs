using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Core.Entities;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Infrastructure.Data;

namespace WorkoutTracker.Infrastructure.Repositories
{
    public class WorkoutSessionRepository : IWorkoutSessionRepository
    {
        private readonly WorkoutDbContext _context;

        public WorkoutSessionRepository(WorkoutDbContext context)
        {
            _context = context;
        }

        private IQueryable<WorkoutSession> GetWorkoutSessionsWithIncludes()
        {
            return _context.WorkoutSessions
              .Include(ws => ws.WorkoutExercises)
                  .ThenInclude(we => we.Sets)
              .Include(ws => ws.WorkoutExercises)
                  .ThenInclude(we => we.Exercise);
        }

        public async Task<WorkoutSession?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await GetWorkoutSessionsWithIncludes()
                .FirstOrDefaultAsync(ws => ws.Id == id, cancellationToken);
        }
        public async Task<IEnumerable<WorkoutSession>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await GetWorkoutSessionsWithIncludes()
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<WorkoutSession>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
        {
            return await GetWorkoutSessionsWithIncludes()
                .Where(ws => start <= ws.PerformedAt && ws.PerformedAt <= end)
                .ToListAsync(cancellationToken);
        }
        public async Task<WorkoutSession> AddAsync(WorkoutSession workoutSession, CancellationToken cancellationToken = default)
        {
            await _context.WorkoutSessions.AddAsync(workoutSession, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return workoutSession;
        }
        public async Task<WorkoutSession> UpdateAsync(WorkoutSession workoutSession, CancellationToken cancellationToken = default)
        {
            workoutSession.UpdatedAt = DateTime.UtcNow;
            _context.WorkoutSessions.Update(workoutSession);
            await _context.SaveChangesAsync(cancellationToken);

            return workoutSession;
        }
        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var exerciseSession = await GetByIdAsync(id, cancellationToken);

            if (exerciseSession == null) return false;

            _context.WorkoutSessions.Remove(exerciseSession);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.WorkoutSessions.AnyAsync(ws => ws.Id == id, cancellationToken);
        }
    }
}