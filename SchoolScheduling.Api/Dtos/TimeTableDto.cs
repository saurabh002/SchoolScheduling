namespace SchoolScheduling.Dtos{

    public record TimetableEntryDto(
        int Id,
        int TeacherId,
        int ClassSectionId,
        string ClassSectionName,
        int PeriodId,
        int PeriodNumber,
        TimeOnly StartTime,
        TimeOnly EndTime,
        DayOfWeek DayOfWeek,
        string? Subject
    );

    public record CreateTimetableEntryDto(
        int TeacherId,
        int ClassSectionId,
        int PeriodId,
        DayOfWeek DayOfWeek,
        string? Subject
    );

    public record UpdateTimetableEntryDto(
        int ClassSectionId,
        int PeriodId,
        DayOfWeek DayOfWeek,
        string? Subject
    );
}