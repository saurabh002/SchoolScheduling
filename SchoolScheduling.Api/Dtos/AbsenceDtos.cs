using System.ComponentModel.DataAnnotations;
namespace SchoolScheduling.Dtos{
public record CreateAbsencePeriodDto(
    int TimetableEntryId,
    int PeriodId
);

public record CreateAbsenceDto(
    int TeacherId,
    DateOnly Date,
    List<CreateAbsencePeriodDto> AffectedPeriods
);

public record AddAbsencePeriodsDto(
    List<CreateAbsencePeriodDto> AffectedPeriods
);

public record AssignSubstituteDto(
    int SubstituteTeacherId
);

public record AvailableSubstituteDto(
    int TeacherId,
    string Name,
    int RegularLoad,
    int SubsTaken,
    int MissedClasses,
    int EffectiveLoad,
    int FreePeriodsToday
);

public record PendingSubstitutionDto(
    int AbsencePeriodId,
    int AbsentTeacherId,
    string AbsentTeacherName,
    string? AbsentTeacherDepartment,
    int PeriodId,
    int PeriodNumber,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string ClassSectionName,
    string? Subject,
    List<AvailableSubstituteDto> AvailableSubstitutes
);

public record AssignedSubstitutionDto(
    int AbsencePeriodId,
    int AbsentTeacherId,
    string AbsentTeacherName,
    int SubstituteTeacherId,
    string SubstituteTeacherName,
    int PeriodNumber,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string ClassSectionName,
    string? Subject
);

public record ExistingAbsenceDto(int Id, List<AbsencePeriodDto> Periods);
public record AbsencePeriodDto(int Id, int TimetableEntryId, bool HasSubstitute);

public record AbsencePeriodRaw(
        DateOnly Date,
        string AbsentTeacher,
        string Class,
        int PeriodNumber,
        int StartTimeHour,
        int StartTimeMinute,
        int EndTimeHour,
        int EndTimeMinute,
        string Substitute
    );
}