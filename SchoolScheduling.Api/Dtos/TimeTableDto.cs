namespace SchoolScheduling.Dtos{
    public record TimetableRowDto(
        DayOfWeek DayOfWeek,
        int PeriodId,
        int ClassSectionId,
        string? Subject
    );
}