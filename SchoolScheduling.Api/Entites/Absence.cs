namespace SchoolScheduling.Entities
{
    // Absence.cs
    public class Absence
    {
        public int Id { get; set; }

        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; } = null!;

        public DateOnly Date { get; set; }

        public ICollection<AbsencePeriod> AffectedPeriods { get; set; } = [];
    }
}