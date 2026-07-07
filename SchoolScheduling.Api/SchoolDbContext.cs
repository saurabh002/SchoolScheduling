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

            modelBuilder.Entity<Period>().HasData(
                new Period { Id = 1, Number = 1, StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(8, 45) },
                new Period { Id = 2, Number = 2, StartTime = new TimeOnly(8, 45), EndTime = new TimeOnly(9, 30) },
                new Period { Id = 3, Number = 3, StartTime = new TimeOnly(9, 30), EndTime = new TimeOnly(10, 15) },
                new Period { Id = 4, Number = 4, StartTime = new TimeOnly(10, 15), EndTime = new TimeOnly(11, 0) },
                new Period { Id = 5, Number = 5, StartTime = new TimeOnly(11, 0), EndTime = new TimeOnly(11, 45) },
                new Period { Id = 6, Number = 6, StartTime = new TimeOnly(11, 45), EndTime = new TimeOnly(12, 30) },
                new Period { Id = 7, Number = 7, StartTime = new TimeOnly(12, 30), EndTime = new TimeOnly(13, 15) },
                new Period { Id = 8, Number = 8, StartTime = new TimeOnly(13, 15), EndTime = new TimeOnly(14, 0) }
            );

            // in your DbContext OnModelCreating, or a seed script
            modelBuilder.Entity<ClassSection>().HasData(
                new ClassSection { Id = 1, Name = "1A" },
                new ClassSection { Id = 2, Name = "1B" },
                new ClassSection { Id = 3, Name = "2A" },
                new ClassSection { Id = 4, Name = "2B" },
                new ClassSection { Id = 5, Name = "3A" },
                new ClassSection { Id = 6, Name = "3B" },
                new ClassSection { Id = 7, Name = "4A" },
                new ClassSection { Id = 8, Name = "4B" },
                new ClassSection { Id = 9, Name = "5A" },
                new ClassSection { Id = 10, Name = "5B" }
            );

        }
    }
}