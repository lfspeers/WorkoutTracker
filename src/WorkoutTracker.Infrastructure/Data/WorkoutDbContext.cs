using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Core.Entities;

namespace WorkoutTracker.Infrastructure.Data
{
    public class WorkoutDbContext : DbContext
    {
        public WorkoutDbContext(DbContextOptions<WorkoutDbContext> options) : base(options){}
        
        // DbSets
        public DbSet<Exercise> Exercises {get; set;}
        public DbSet<WorkoutSession> WorkoutSessions {get; set;}
        public DbSet<WorkoutExercise> WorkoutExercises {get; set;}
        public DbSet<Set> Sets {get; set;}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkoutDbContext).Assembly);
        }
    }
}