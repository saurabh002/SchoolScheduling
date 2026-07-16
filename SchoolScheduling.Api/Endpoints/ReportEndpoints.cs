using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SchoolScheduling.Dtos;

namespace SchoolScheduling.Endpoints
{
    public static class ReportEndpoints
    {
        public static RouteGroupBuilder MapReportEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/reports").WithTags("Reports");
    
            group.MapGet("/summary", GetSummary)
                .WithName("GetReportSummary")
                .Produces<ReportSummaryDto>(StatusCodes.Status200OK);
    
            return group;
        }
    
        private static async Task<Ok<ReportSummaryDto>> GetSummary(
            SchoolDbContext db,
            // periodType: "week" | "month"
            // offset: 1 = last completed period, 2 = two periods ago, etc.
            string periodType,
            int offset,
            CancellationToken ct)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var trendStart = today.AddDays(-6);
    
            // Compute period date range and label based on periodType + offset
            DateOnly periodStart;
            DateOnly periodEnd;
            string periodLabel;
    
            if (periodType == "month")
            {
                // Last completed calendar month = offset months back
                var targetMonth = today.AddMonths(-offset);
                periodStart = new DateOnly(targetMonth.Year, targetMonth.Month, 1);
                periodEnd = periodStart.AddMonths(1).AddDays(-1);
                periodLabel = periodStart.ToString("MMMM yyyy");
            }
            else
            {
                // Last completed Mon-Sun week = offset weeks back
                // Find last completed Sunday first
                var daysSinceSunday = ((int)today.DayOfWeek == 0 ? 7 : (int)today.DayOfWeek);
                var lastSunday = today.AddDays(-daysSinceSunday);
                periodEnd = lastSunday.AddDays(-(7 * (offset - 1)));
                periodStart = periodEnd.AddDays(-6);
                periodLabel = $"{periodStart:d MMM} – {periodEnd:d MMM yyyy}";
            }
    
            // Always-live stat cards
            var totalTeachers = await db.Teachers.CountAsync(ct);
    
            var absencesToday = await db.Absences
                .CountAsync(a => a.Date == today, ct);
    
            var weekStart = today.AddDays(-((int)today.DayOfWeek == 0 ? 7 : (int)today.DayOfWeek));
            var absencesThisWeek = await db.Absences
                .CountAsync(a => a.Date >= weekStart && a.Date <= today, ct);
    
            var periodsToday = await db.AbsencePeriods
                .Where(ap => ap.Absence.Date == today)
                .ToListAsync(ct);
    
            var periodsPendingToday = periodsToday.Count(p => p.SubstituteTeacherId == null);
            var periodsAssignedToday = periodsToday.Count(p => p.SubstituteTeacherId != null);
    
            // Effective workload for the selected period
            var rawTeachers = await db.Teachers
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    RegularLoad = t.TimetableEntries.Count(),
                    SubsTaken = db.AbsencePeriods.Count(ap =>
                        ap.SubstituteTeacherId == t.Id &&
                        ap.Absence.Date >= periodStart &&
                        ap.Absence.Date <= periodEnd),
                    MissedClasses = db.AbsencePeriods.Count(ap =>
                        ap.Absence.TeacherId == t.Id &&
                        ap.Absence.Date >= periodStart &&
                        ap.Absence.Date <= periodEnd),
                })
                .ToListAsync(ct);
    
            var topSubstitutes = rawTeachers
                .Select(t => new SubstituteWorkloadDto(
                    t.Id,
                    t.Name,
                    t.RegularLoad,
                    t.SubsTaken,
                    t.MissedClasses,
                    t.RegularLoad + t.SubsTaken - t.MissedClasses))
                .OrderByDescending(t => t.EffectiveLoad)
                .Take(5)
                .ToList();
    
            // Rolling 7-day absence trend (always live, not period-bound)
            var trendRaw = await db.Absences
                .Where(a => a.Date >= trendStart && a.Date <= today)
                .GroupBy(a => a.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync(ct);
    
            var absenceTrend = Enumerable.Range(0, 7)
                .Select(i => trendStart.AddDays(i))
                .Select(date => new AbsenceTrendPointDto(
                    date,
                    trendRaw.FirstOrDefault(t => t.Date == date)?.Count ?? 0))
                .ToList();
    
            var dto = new ReportSummaryDto(
                totalTeachers,
                absencesToday,
                absencesThisWeek,
                periodsPendingToday,
                periodsAssignedToday,
                periodLabel,
                topSubstitutes,
                absenceTrend);
    
            return TypedResults.Ok(dto);
        }
    }
}