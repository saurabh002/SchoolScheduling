using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SchoolScheduling.Dtos;
using SchoolScheduling.Entities;

namespace SchoolScheduling.Endpoints{
    public static class TeacherEndpoints{

        public static void MapTeacherEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/teachers").WithTags("Teachers");

            group.MapPost("/", CreateTeacher);
            group.MapGet("/{id:int}", GetTeacher);
            group.MapGet("/", GetTeachers);
        }

        private static async Task<Results<Created<TeacherDto>, ValidationProblem, Conflict<string>>> CreateTeacher(
            CreateTeacherDto dto, SchoolDbContext db)
        {
            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                var teacher = new Teacher
                {
                    Name = dto.Name.Trim(),
                    Department = dto.Department?.Trim(),
                    Email = dto.Email?.Trim().ToLowerInvariant(),
                    Phone = dto.Phone?.Trim(),
                    IsActive = true
                };
                db.Teachers.Add(teacher);
                await db.SaveChangesAsync(); // need teacher.Id before adding schedule rows

                foreach (var row in dto.Schedule)
                {
                    db.TimetableEntries.Add(new TimetableEntry
                    {
                        TeacherId = teacher.Id,
                        DayOfWeek = row.DayOfWeek,
                        PeriodId = row.PeriodId,
                        ClassSectionId = row.ClassSectionId,
                        Subject = row.Subject
                    });
                }
                await db.SaveChangesAsync(); // throws if unique constraint violated (double-booked slot)

                await transaction.CommitAsync();

                var result = new TeacherDto(teacher.Id, teacher.Name, teacher.Department, teacher.Email, teacher.Phone, teacher.IsActive);
                return TypedResults.Created($"/api/teachers/{teacher.Id}", result);
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                // Hits this if a Day+Period combo is already taken - either by this same teacher twice,
                // or because that class section already has someone else scheduled then.
                return TypedResults.Conflict("One or more schedule slots conflict with an existing entry.");
            }
        }
        private static async Task<Ok<List<TeacherDto>>> GetTeachers(SchoolDbContext db, bool activeOnly = true)
        {
            var query = db.Teachers.AsQueryable();
            if (activeOnly) query = query.Where(t => t.IsActive);

            var teachers = await query
                .Select(t => new TeacherDto(t.Id, t.Name, t.Department, t.Email, t.Phone, t.IsActive))
                .ToListAsync();

            return TypedResults.Ok(teachers);
        }

        private static async Task<Results<Ok<TeacherDto>, NotFound>> GetTeacher(int id, SchoolDbContext db)
        {
            var teacher = await db.Teachers.FindAsync(id);
            if (teacher is null) return TypedResults.NotFound();

            return TypedResults.Ok(new TeacherDto(teacher.Id, teacher.Name, teacher.Department, teacher.Email, teacher.Phone, teacher.IsActive));
        }
    }
}