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
        }
    }
}