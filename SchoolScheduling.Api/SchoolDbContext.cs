using Microsoft.EntityFrameworkCore;
using SchoolScheduling.Entities;

namespace SchoolScheduling
{
    // SchoolDbContext.cs
    public class SchoolDbContext(DbContextOptions<SchoolDbContext> options) : DbContext(options)
    {
        public DbSet<Teacher> Teachers => Set<Teacher>();
        public DbSet<ClassSection> ClassSections => Set<ClassSection>();
        public DbSet<Period> Periods => Set<Period>();
        public DbSet<TimetableEntry> TimetableEntries => Set<TimetableEntry>();
        public DbSet<Absence> Absences => Set<Absence>();
        public DbSet<AbsencePeriod> AbsencePeriods => Set<AbsencePeriod>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TimetableEntry>()
                .HasIndex(t => new { t.TeacherId, t.DayOfWeek, t.PeriodId })
                .IsUnique();

            modelBuilder.Entity<TimetableEntry>()
                .HasIndex(t => new { t.ClassSectionId, t.DayOfWeek, t.PeriodId })
                .IsUnique();

            modelBuilder.Entity<AbsencePeriod>()
                .HasIndex(a => new { a.AbsenceId, a.TimetableEntryId })
                .IsUnique();

            modelBuilder.Entity<AbsencePeriod>()
                .HasIndex(a => new { a.SubstituteTeacherId, a.Date, a.PeriodId })
                .IsUnique()
                .HasFilter("[SubstituteTeacherId] IS NOT NULL");

            modelBuilder.Entity<AbsencePeriod>()
                .HasOne(a => a.SubstituteTeacher)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Absence>()
                .HasMany(a => a.AffectedPeriods)
                .WithOne(ap => ap.Absence)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Absence>()
                .HasIndex(a => new { a.TeacherId, a.Date })
                .IsUnique();

            modelBuilder.Entity<Period>().HasData(
                new Period { Id = 1, Number = 1, StartTime = new TimeOnly(8, 30), EndTime = new TimeOnly(9, 05) },
                new Period { Id = 2, Number = 2, StartTime = new TimeOnly(9, 05), EndTime = new TimeOnly(9, 40) },
                new Period { Id = 3, Number = 3, StartTime = new TimeOnly(9, 55), EndTime = new TimeOnly(10, 35) },
                new Period { Id = 4, Number = 4, StartTime = new TimeOnly(10, 35), EndTime = new TimeOnly(11, 05) },
                new Period { Id = 5, Number = 5, StartTime = new TimeOnly(11, 05), EndTime = new TimeOnly(11, 40) },
                new Period { Id = 6, Number = 6, StartTime = new TimeOnly(11, 40), EndTime = new TimeOnly(12, 15) },
                new Period { Id = 7, Number = 7, StartTime = new TimeOnly(12, 45), EndTime = new TimeOnly(13, 20) },
                new Period { Id = 8, Number = 8, StartTime = new TimeOnly(13, 20), EndTime = new TimeOnly(13, 55) },
                new Period { Id = 9, Number = 9, StartTime = new TimeOnly(13, 55), EndTime = new TimeOnly(14, 35) }
            );

            // in your DbContext OnModelCreating, or a seed script
            modelBuilder.Entity<ClassSection>().HasData(
                new ClassSection { Id = 1, Name = "1A" },
                new ClassSection { Id = 2, Name = "1B" },
                new ClassSection { Id = 3, Name = "1C" },
                new ClassSection { Id = 4, Name = "1D" },
                new ClassSection { Id = 5, Name = "1E" },
                new ClassSection { Id = 6, Name = "2A" },
                new ClassSection { Id = 7, Name = "2B" },
                new ClassSection { Id = 8, Name = "2C" },
                new ClassSection { Id = 9, Name = "2D" },
                new ClassSection { Id = 10, Name = "2E" },
                new ClassSection { Id = 11, Name = "2F" },
                new ClassSection { Id = 12, Name = "3A" },
                new ClassSection { Id = 13, Name = "3B" },
                new ClassSection { Id = 14, Name = "3C" },
                new ClassSection { Id = 15, Name = "3D" },
                new ClassSection { Id = 16, Name = "3E" },
                new ClassSection { Id = 17, Name = "3F" },
                new ClassSection { Id = 18, Name = "4A" },
                new ClassSection { Id = 19, Name = "4B" },
                new ClassSection { Id = 20, Name = "4C" },
                new ClassSection { Id = 21, Name = "4D" },
                new ClassSection { Id = 22, Name = "4E" },
                new ClassSection { Id = 23, Name = "5F" },
                new ClassSection { Id = 24, Name = "5A" },
                new ClassSection { Id = 25, Name = "5B" },
                new ClassSection { Id = 26, Name = "5C" },
                new ClassSection { Id = 27, Name = "5D" },
                new ClassSection { Id = 28, Name = "5E" },
                new ClassSection { Id = 29, Name = "5F" }
            );

        }
    }
}