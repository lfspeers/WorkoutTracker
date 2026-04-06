using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutTracker.Core.Entities;

namespace WorkoutTracker.Infrastructure.Data.Configurations
{
    public class WorkoutSessionConfiguration : IEntityTypeConfiguration<WorkoutSession>
    {
        public void Configure(EntityTypeBuilder<WorkoutSession> builder)
        {
            builder.HasIndex(w => w.PerformedAt);
            builder.Property(w => w.Notes).HasMaxLength(500);
        }
    }
}