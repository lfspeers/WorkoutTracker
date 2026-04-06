using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutTracker.Core.Entities;

namespace WorkoutTracker.Infrastructure.Data.Configurations
{
    public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
    {
        public void Configure(EntityTypeBuilder<Exercise> builder)
        {
            builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
            builder.HasIndex(e => e.Name).IsUnique();
            builder.Property(e => e.Category).HasConversion<string>();
            builder.Property(e => e.MuscleGroup).HasMaxLength(50);
            builder.Property(e => e.Description).HasMaxLength(500);
        }
    }
}