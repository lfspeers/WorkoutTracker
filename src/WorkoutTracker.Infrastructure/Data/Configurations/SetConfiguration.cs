using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutTracker.Core.Entities;

namespace WorkoutTracker.Infrastructure.Data.Configurations
{
    public class SetConfiguration : IEntityTypeConfiguration<Set>
    {
        public void Configure(EntityTypeBuilder<Set> builder)
        {
            builder.Property(s => s.Notes).HasMaxLength(200);
            builder.Property(s => s.Weight).HasPrecision(8, 2);
            builder.HasOne(s => s.WorkoutExercise)
                .WithMany(we => we.Sets)
                .HasForeignKey(s => s.WorkoutExerciseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}