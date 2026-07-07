using Microsoft.EntityFrameworkCore;
using SchoolScheduling.Dtos;
using SchoolScheduling.Entities;

namespace SchoolScheduling.Endpoints
{
    public static class AbsenceEndpoints
    {
        public static void MapAbsenceEndpoints(this IEndpointRouteBuilder app)
        {
            var absenceGroup = app.MapGroup("/api/absences").WithTags("Absences");

            absenceGroup.MapPost("/", CreateAbsence);

            var absencePeriodGroup = app.MapGroup("/api/absence-periods").WithTags("Absences");
            absencePeriodGroup.MapPut("/{id}/assign", AssignSubstitute);

            var substituteGroup = app.MapGroup("/api/substitutes").WithTags("Substitutions");
            substituteGroup.MapGet("/assigned", GetAssignedSubstitutes);
            substituteGroup.MapGet("/pending", GetPendingSubstitutes);
        }

        private static async Task<IResult> GetPendingSubstitutes(DateOnly date, SchoolDbContext db)
        {
            var dayOfWeek = date.DayOfWeek;

            var pending = await db.AbsencePeriods
            .Where(ap => ap.Date == date && ap.SubstituteTeacherId == null)
            .Select(ap => new
            {
                ap.Id,
                ap.PeriodId,
                ClassSectionName = ap.TimetableEntry.ClassSection.Name,
                ap.TimetableEntry.Subject,
                PeriodNumber = ap.TimetableEntry.Period.Number,
                ap.TimetableEntry.Period.StartTime,
                ap.TimetableEntry.Period.EndTime,
                AbsentTeacherId = ap.Absence.TeacherId,
                AbsentTeacherName = ap.Absence.Teacher.Name,
                AbsentTeacherDepartment = ap.Absence.Teacher.Department
            })
            .ToListAsync();

            var result = new List<PendingSubstitutionDto>();

            foreach (var ap in pending)
            {
                var availableSubs = await db.Teachers
                    .Where(t =>
                        t.IsActive &&
                        t.Id != ap.AbsentTeacherId &&
                        !db.TimetableEntries.Any(te =>
                            te.TeacherId == t.Id &&
                            te.PeriodId == ap.PeriodId &&
                            te.DayOfWeek == dayOfWeek))
                    .Select(t => new
                    {
                        t.Id,
                        t.Name,
                        RegularLoad = db.TimetableEntries.Count(te => te.TeacherId == t.Id),
                        SubsTaken = db.AbsencePeriods.Count(a => a.SubstituteTeacherId == t.Id),
                        MissedClasses = db.AbsencePeriods.Count(a => a.TimetableEntry.TeacherId == t.Id)
                    })
                    .Select(x => new
                    {
                        x.Id,
                        x.Name,
                        x.RegularLoad,
                        x.SubsTaken,
                        x.MissedClasses,
                        EffectiveLoad = x.RegularLoad + x.SubsTaken - x.MissedClasses
                    })
                    .OrderBy(x => x.EffectiveLoad)
                    .Select(x => new AvailableSubstituteDto(
                        x.Id,
                        x.Name,
                        x.RegularLoad,
                        x.SubsTaken,
                        x.MissedClasses,
                        x.EffectiveLoad
                    ))
                    .ToListAsync();

                result.Add(new PendingSubstitutionDto(
                    ap.Id,
                    ap.AbsentTeacherId,
                    ap.AbsentTeacherName,
                    ap.AbsentTeacherDepartment,
                    ap.PeriodId,
                    ap.PeriodNumber,
                    ap.StartTime,
                    ap.EndTime,
                    ap.ClassSectionName,
                    ap.Subject,
                    availableSubs
                ));
            }

            return Results.Ok(result);
        }

        private static async Task<IResult> GetAssignedSubstitutes(DateOnly date, SchoolDbContext db)
        {
            var assigned = await db.AbsencePeriods
            .Where(ap => ap.Date == date && ap.SubstituteTeacherId != null)
            .Select(ap => new AssignedSubstitutionDto(
                ap.Id,
                ap.Absence.TeacherId,
                ap.Absence.Teacher.Name,
                ap.SubstituteTeacherId!.Value,
                ap.SubstituteTeacher!.Name,
                ap.TimetableEntry.Period.Number,
                ap.TimetableEntry.Period.StartTime,
                ap.TimetableEntry.Period.EndTime,
                ap.TimetableEntry.ClassSection.Name,
                ap.TimetableEntry.Subject
            ))
            .OrderBy(ap => ap.PeriodNumber)
            .ToListAsync();

            return Results.Ok(assigned);
        }

        private static async Task<IResult> AssignSubstitute(int id, AssignSubstituteDto dto, SchoolDbContext db)
        {
            var period = await db.AbsencePeriods.FindAsync(id);
            if (period is null) { return Results.NotFound(); }

            var dayOfWeek = period.Date.DayOfWeek;

            var conflictingSubstitution = await db.TimetableEntries
                .AnyAsync(te => te.DayOfWeek == dayOfWeek 
                && te.PeriodId == period.PeriodId 
                && te.TeacherId == dto.SubstituteTeacherId);

            if (conflictingSubstitution)
            {
                return Results.BadRequest(new { message = "The substitute teacher has a conflicting class during this period." });
            }
                
            period.SubstituteTeacherId = dto.SubstituteTeacherId;
            await db.SaveChangesAsync();
            return Results.Ok(new { period.Id, period.Absence.TeacherId, period.SubstituteTeacherId });
        }

        private static async Task<IResult> CreateAbsence(CreateAbsenceDto dto, SchoolDbContext db)
        {
            var absence = new Absence
            {
                TeacherId = dto.TeacherId,
                Date = dto.Date,
                AffectedPeriods = dto.AffectedPeriods.Select(p => new AbsencePeriod
                {
                    TimetableEntryId = p.TimetableEntryId,
                    PeriodId = p.PeriodId,
                    Date = dto.Date
                }).ToList()
            };
            
            db.Absences.Add(absence);
            await db.SaveChangesAsync();

            return Results.Created($"/api/absences/{absence.Id}", new { absence.Id });
        }
    }
}