namespace SchoolScheduling.Dtos;
public record TodaySubstitutionDto(
    int PeriodNumber,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string ClassName,
    string? Subject,
    string AbsentTeacherName
);