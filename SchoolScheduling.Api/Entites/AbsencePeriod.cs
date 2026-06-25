namespace SchoolScheduling.Entities
{
    // AbsencePeriod.cs
    public class AbsencePeriod
    {
        public int Id { get; set; }

        public int AbsenceId { get; set; }
        public Absence Absence { get; set; } = null!;

        public int TimetableEntryId { get; set; }
        public TimetableEntry TimetableEntry { get; set; } = null!;

        public int? SubstituteTeacherId { get; set; }
        public Teacher? SubstituteTeacher { get; set; }

        public DateOnly Date { get; set; }
        public int PeriodId { get; set; }
    }
}