using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutTracker.Core.Entities;

namespace WorkoutTracker.Infrastructure.Data.Configurations
{
    public class WorkoutExerciseConfiguration : IEntityTypeConfiguration<WorkoutExercise>
    {
        public void Configure(EntityTypeBuilder<WorkoutExercise> builder)
        {
            builder.Property(w => w.Notes).HasMaxLength(200);
            builder.Property(w => w.WeightUnit).HasMaxLength(3);
            builder.HasOne(we => we.WorkoutSession)
                .WithMany(ws => ws.WorkoutExercises)
                .HasForeignKey(we => we.WorkoutSessionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(we => we.Exercise)
                .WithMany(e => e.WorkoutExercises)
                .HasForeignKey(we => we.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict); // Blocks delete if exercise is in use
        }
    }
}