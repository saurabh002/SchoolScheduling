using System.ComponentModel.DataAnnotations;

namespace SchoolScheduling.Entities
{
    // TimetableEntry.cs
    public class TimetableEntry
    {
        public int Id { get; set; }

        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; } = null!;

        public int ClassSectionId { get; set; }
        public ClassSection ClassSection { get; set; } = null!;

        public int PeriodId { get; set; }
        public Period Period { get; set; } = null!;

        public DayOfWeek DayOfWeek { get; set; }
        public string? Subject { get; set; }
    }
}