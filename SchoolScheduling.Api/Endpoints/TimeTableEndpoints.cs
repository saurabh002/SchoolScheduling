using SchoolScheduling.Dtos;
using SchoolScheduling.Entities;
using Microsoft.EntityFrameworkCore;

namespace SchoolScheduling.Endpoints{
    public static class TimeTableEndpoints
    {
        public static void MapTimeTableEndpoints(this IEndpointRouteBuilder app)
        {
            var teacherGroup = app.MapGroup("/api/teachers").WithTags("TimeTable");
            teacherGroup.MapGet("/{teacherId}/timetable", GetTimeTableByTeacherId);

            var group = app.MapGroup("/api/timetable").WithTags("TimeTable");
            group.MapPost("/", CreateTimetableEntry);
            group.MapDelete("/{id}", DeleteTimetableEntry);
            group.MapPut("/{id}", UpdateTimetableEntry);
        }

        private static async Task<IResult> GetTimeTableByTeacherId(int teacherId, SchoolDbContext db)
        {
            var timetable = await db.TimetableEntries
                .Where(t => t.TeacherId == teacherId)
                .Select(t => new TimetableEntryDto(t.Id, t.TeacherId, t.ClassSectionId, t.ClassSection.Name, t.PeriodId, t.Period.Number, t.Period.StartTime, t.Period.EndTime, t.DayOfWeek, t.Subject))
                .ToListAsync();

            return Results.Ok(timetable);
        }   

    private static async Task<IResult> CreateTimetableEntry(CreateTimetableEntryDto dto, SchoolDbContext db)
    {
        var classConflict = await db.TimetableEntries.AnyAsync(t =>
            t.ClassSectionId == dto.ClassSectionId &&
            t.PeriodId == dto.PeriodId &&
            t.DayOfWeek == dto.DayOfWeek);

        if (classConflict)
            return Results.Conflict(new { message = "This class/period/day slot is already assigned to another teacher." });

        var entry = new TimetableEntry
        {
            TeacherId = dto.TeacherId,
            ClassSectionId = dto.ClassSectionId,
            PeriodId = dto.PeriodId,
            DayOfWeek = dto.DayOfWeek,
            Subject = dto.Subject
        };

        db.TimetableEntries.Add(entry);
        await db.SaveChangesAsync();
        return Results.Created($"/api/timetable/{entry.Id}", entry);
    }

    private static async Task<IResult> UpdateTimetableEntry(int id, UpdateTimetableEntryDto dto, SchoolDbContext db)
    {
        var entry = await db.TimetableEntries.FindAsync(id);
        if (entry is null) return Results.NotFound();

        var classConflict = await db.TimetableEntries.AnyAsync(t =>
            t.Id != id &&
            t.ClassSectionId == dto.ClassSectionId &&
            t.PeriodId == dto.PeriodId &&
            t.DayOfWeek == dto.DayOfWeek);

        if (classConflict)
            return Results.Conflict(new { message = "This class/period/day slot is already assigned to another teacher." });

        entry.ClassSectionId = dto.ClassSectionId;
        entry.PeriodId = dto.PeriodId;
        entry.DayOfWeek = dto.DayOfWeek;
        entry.Subject = dto.Subject;

        await db.SaveChangesAsync();
        return Results.Ok(entry);
    }

    private static async Task<IResult> DeleteTimetableEntry(int id, SchoolDbContext db)
    {
        var entry = await db.TimetableEntries.FindAsync(id);
        if (entry is null) return Results.NotFound();

        db.TimetableEntries.Remove(entry);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }    
    }
}