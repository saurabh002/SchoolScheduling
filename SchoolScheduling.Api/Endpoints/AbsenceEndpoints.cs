using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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
            absenceGroup.MapPost("/absences/{id}/periods", AddAbsencePeriods);
            absenceGroup.MapDelete("/absences/{id}", DeleteAbsence);
            absenceGroup.MapDelete("/absence-periods/{absencePeriodId}", DeleteAbsenceByPeriodId);
            absenceGroup.MapGet("absences", GetAbsences);

            var absencePeriodGroup = app.MapGroup("/api/absence-periods").WithTags("Absences");
            absencePeriodGroup.MapPut("/{id}/assign", AssignSubstitute);

            var substituteGroup = app.MapGroup("/api/substitutes").WithTags("Substitutions");
            substituteGroup.MapGet("/assigned", GetAssignedSubstitutes);
            substituteGroup.MapGet("/pending", GetPendingSubstitutes);
            substituteGroup.MapDelete("/{absencePeriodId}/assign", CancelAssignment);
            substituteGroup.MapGet("/today/{id}", GetTodaySubstitutions);
        }

        private static async Task<IResult> GetAbsences(int teacherId, DateOnly date, SchoolDbContext db)
        {
            var absence = await db.Absences
            .Include(a => a.AffectedPeriods)
            .FirstOrDefaultAsync(a => a.TeacherId == teacherId && a.Date == date);

            if (absence is null) return Results.NotFound();

            return Results.Ok(new ExistingAbsenceDto(
                absence.Id,
                absence.AffectedPeriods.Select(ap => new AbsencePeriodDto(
                    ap.Id,
                    ap.TimetableEntryId,
                    ap.SubstituteTeacherId != null
                )).ToList()
            ));
        }

        private static async Task<IResult> CancelAssignment(int absencePeriodId, SchoolDbContext db)
        {
            var period = await db.AbsencePeriods.FindAsync(absencePeriodId);
            if (period is null) return Results.NotFound();
            if (period.SubstituteTeacherId is null) return Results.BadRequest("No substitute assigned.");

            period.SubstituteTeacherId = null;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }

        private static async Task<IResult> AddAbsencePeriods(int id, AddAbsencePeriodsDto dto, SchoolDbContext db)
        {
            var absence = await db.Absences
                .Include(a => a.AffectedPeriods)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (absence is null) return Results.NotFound();
            if (dto.AffectedPeriods is null || dto.AffectedPeriods.Count == 0)
                return Results.BadRequest(new { message = "At least one period is required." });

            var existingTimetableEntries = absence.AffectedPeriods
                .Select(ap => ap.TimetableEntryId)
                .ToHashSet();

            var toAdd = dto.AffectedPeriods
                .DistinctBy(p => p.TimetableEntryId)
                .Where(p => !existingTimetableEntries.Contains(p.TimetableEntryId))
                .Select(p => new AbsencePeriod
                {
                    AbsenceId = absence.Id,
                    TimetableEntryId = p.TimetableEntryId,
                    PeriodId = p.PeriodId,
                    Date = absence.Date
                })
                .ToList();

            if (toAdd.Count == 0)
                return Results.Ok(new { absence.Id, addedCount = 0 });

            db.AbsencePeriods.AddRange(toAdd);
            await db.SaveChangesAsync();

            return Results.Ok(new { absence.Id, addedCount = toAdd.Count });
        }

        private static async Task<IResult> DeleteAbsence(int id, SchoolDbContext db)
        {
            var absence = await db.Absences
                .Include(a => a.AffectedPeriods)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (absence is null) return Results.NotFound();

            bool hasAssignments = absence.AffectedPeriods
                .Any(ap => ap.SubstituteTeacherId != null);

            if (hasAssignments)
                return Results.Conflict(new { 
                    message = "Cannot delete — substitutions already assigned. Remove them first." 
                });

            db.Absences.Remove(absence); // cascade removes AbsencePeriods via EF
            await db.SaveChangesAsync();
            return Results.NoContent();
        }

        private static async Task<IResult> DeleteAbsenceByPeriodId(int absencePeriodId, SchoolDbContext db)
        {
            var period = await db.AbsencePeriods
                .FirstOrDefaultAsync(ap => ap.Id == absencePeriodId);

            if (period is null) return Results.NotFound();

            if (period.SubstituteTeacherId is not null)
                return Results.Conflict(new {
                    message = "Substitute already assigned for this period. Please unassign first."
                });

            db.AbsencePeriods.Remove(period);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }

        private static async Task<IResult> GetPendingSubstitutes(DateOnly date, SchoolDbContext db)
        {
            var allPeriods = await db.AbsencePeriods.ToListAsync();
            Console.WriteLine($"Total AbsencePeriods: {allPeriods.Count}");
            Console.WriteLine($"Date: {date}");
            foreach (var p in allPeriods)
            {
                Console.WriteLine($"Id: {p.Id}, Date: {p.Date}, SubId: {p.SubstituteTeacherId}");
            }

            var dayOfWeek = date.DayOfWeek;
            
            //var today = DateOnly.FromDateTime(DateTime.Today);
            var todayDow = date.DayOfWeek;
            var totalPeriodsToday = await db.TimetableEntries
                    .Where(te => te.DayOfWeek == dayOfWeek)
                    .Select(te => te.PeriodId)
                    .Distinct()
                    .CountAsync();

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
            Console.WriteLine($"Total Pending AbsencePeriods: {pending.Count}");
            var result = new List<PendingSubstitutionDto>();
            Console.WriteLine($"Date parameter: {date}");
            var allDates = await db.AbsencePeriods.Select(ap => ap.Date).ToListAsync();
            foreach (var d in allDates)
            {
                Console.WriteLine($"AbsencePeriod date: {d}");
            }
            foreach (var ap in pending)
            {
                var totalTeachers = await db.Teachers.CountAsync(t => t.IsActive);
                Console.WriteLine($"Active teachers: {totalTeachers}");

                var excludingAbsent = await db.Teachers
                    .CountAsync(t => t.IsActive && t.Id != ap.AbsentTeacherId);
                Console.WriteLine($"Excluding absent: {excludingAbsent}");

                // var freeTeachers = await db.Teachers
                //     .Where(t =>
                //         t.IsActive &&
                //         t.Id != ap.AbsentTeacherId &&
                //         !db.TimetableEntries.Any(te =>
                //             te.TeacherId == t.Id &&
                //             te.PeriodId == ap.PeriodId &&
                //             te.DayOfWeek == dayOfWeek))
                //     .Select(t => new
                //     {
                //         t.Id,
                //         t.Name,
                //         RegularLoad = db.TimetableEntries.Count(te => te.TeacherId == t.Id),
                //         SubsTaken = db.AbsencePeriods.Count(a => a.SubstituteTeacherId == t.Id),
                //         MissedClasses = db.AbsencePeriods.Count(a => a.TimetableEntry.TeacherId == t.Id)
                //     }).ToListAsync();

                // Console.WriteLine($"Free teachers: {freeTeachers}");

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
                            x.EffectiveLoad,
                            totalPeriodsToday
                            - db.TimetableEntries.Count(te => te.TeacherId == x.Id && te.DayOfWeek == todayDow)
                            - db.AbsencePeriods.Count(ap => ap.SubstituteTeacherId == x.Id && ap.Date == date)
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
            Console.WriteLine($"Total Pending Substitutions: {result.Count}");
            Console.WriteLine($"Result: {System.Text.Json.JsonSerializer.Serialize(result)}");
            return Results.Ok(result);
        }

        private static async Task<IResult> GetAssignedSubstitutes(DateOnly date, SchoolDbContext db)
        {
            var assigned = await db.AbsencePeriods
            .Where(ap => ap.Date == date && ap.SubstituteTeacherId != null)
            .OrderBy(ap => ap.TimetableEntry.Period.Number)
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
            .ToListAsync();

            return Results.Ok(assigned);
        }

        private static async Task<IResult> AssignSubstitute(int id, [FromBody]AssignSubstituteDto dto, SchoolDbContext db)
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
            Console.WriteLine($"Period:  {System.Text.Json.JsonSerializer.Serialize(period)}");
            return Results.Ok(new { period.Id, period.SubstituteTeacherId });
        }

        private static async Task<IResult> CreateAbsence(CreateAbsenceDto dto, SchoolDbContext db)
        {
            // duplicate check
            bool exists = await db.Absences.AnyAsync(a =>
            a.TeacherId == dto.TeacherId && a.Date == dto.Date);

            if (exists)
            {
                return Results.Conflict(new { 
                    message = "Teacher already marked absent on this date." 
                });
            }

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

        private static async Task<Ok<List<TodaySubstitutionDto>>> GetTodaySubstitutions(
        int id,
        SchoolDbContext db,
        CancellationToken ct)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
    
            var substitutions = await db.AbsencePeriods
                .Where(ap =>
                    ap.SubstituteTeacherId == id &&
                    ap.Date == today)
                .OrderBy(ap => ap.TimetableEntry.Period.Number)
                .Select(ap => new TodaySubstitutionDto(
                    ap.TimetableEntry.Period.Number,
                    ap.TimetableEntry.Period.StartTime,
                    ap.TimetableEntry.Period.EndTime,
                    ap.TimetableEntry.ClassSection.Name,
                    ap.TimetableEntry.Subject,
                    ap.Absence.Teacher.Name
                ))
                .ToListAsync(ct);
    
            return TypedResults.Ok(substitutions);
        }
    }
}