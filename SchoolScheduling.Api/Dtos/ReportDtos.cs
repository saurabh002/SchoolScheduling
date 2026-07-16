namespace SchoolScheduling.Dtos;

public record ReportSummaryDto(
    int TotalTeachers,
    int AbsencesToday,
    int AbsencesThisWeek,
    int PeriodsPendingToday,
    int PeriodsAssignedToday,
    string PeriodLabel,
    List<SubstituteWorkloadDto> TopSubstitutes,
    List<AbsenceTrendPointDto> AbsenceTrend
);
 
public record SubstituteWorkloadDto(
    int TeacherId,
    string Name,
    int RegularLoad,
    int SubsTaken,
    int MissedClasses,
    int EffectiveLoad
);
 
public record AbsenceTrendPointDto(
    DateOnly Date,
    int Count
);
 
// Top teachers by subs taken in a period (weekly / monthly)
public record TopSubstitutePeriodDto(
    int TeacherId,
    string Name,
    int SubsTaken
);
