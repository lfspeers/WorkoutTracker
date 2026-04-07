using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Core.Entities;
using WorkoutTracker.Core.Enums;
using WorkoutTracker.Core.Interfaces;
using WorkoutTracker.Infrastructure.Data;

namespace WorkoutTracker.Infrastructure.Repositories
{
    public class ExerciseRepository : IExerciseRepository
    {
        private readonly WorkoutDbContext _context;

        public ExerciseRepository(WorkoutDbContext context)
        {
            _context = context;
        }
        public async Task<Exercise?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Exercises.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }
        public async Task<IEnumerable<Exercise>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Exercises.ToListAsync(cancellationToken);
        }
        public async Task<IEnumerable<Exercise>> GetByCategoryAsync(ExerciseCategory exerciseCategory, CancellationToken cancellationToken = default)
        {
            return await _context.Exercises
                .Where(e => e.Category == exerciseCategory)
                .ToListAsync(cancellationToken);
        }
        public async Task<Exercise> AddAsync(Exercise exercise, CancellationToken cancellationToken = default)
        {
            await _context.Exercises.AddAsync(exercise, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            
            return exercise;
        }
        public async Task<Exercise> UpdateAsync(Exercise exercise, CancellationToken cancellationToken = default)
        {
            _context.Exercises.Update(exercise);
            await _context.SaveChangesAsync(cancellationToken);

            return exercise;
        }
        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            Exercise? exercise = await GetByIdAsync(id, cancellationToken);

            if (exercise == null) return false;

            _context.Exercises.Remove(exercise);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Exercises.AnyAsync(e => e.Id == id, cancellationToken);
        }
    }

}