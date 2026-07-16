using Microsoft.EntityFrameworkCore;
using SchoolScheduling.Dtos;
using SchoolScheduling.Entities;

namespace SchoolScheduling.Endpoints{
    public static class TeacherEndpoints
    {   
        public static void MapTeacherEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/teachers").WithTags("Teachers");

            group.MapGet("/", GetAllTeachers);
            group.MapGet("/{id}", GetTeacherById);
            group.MapPost("/", CreateTeacher);
            // group.MapPut("/{id}", UpdateTeacher);
            group.MapDelete("/{id}", DeleteTeacher);
        }

        private static async Task<IResult> GetAllTeachers(SchoolDbContext db)
        {
            var teachers = await db.Teachers
                .Select(t => new TeacherDto(t.Id, t.Name, t.Department, t.Email, t.Phone, t.IsActive))
                .ToListAsync();

            return Results.Ok(teachers);
        }

        private static async Task<IResult> GetTeacherById(int id, SchoolDbContext db)
        {
            var teacher = await db.Teachers
                .Where(t => t.Id == id)
                .Select(t => new TeacherDto(t.Id, t.Name, t.Department, t.Email, t.Phone, t.IsActive))
                .FirstOrDefaultAsync();

            return teacher is null ? Results.NotFound() : Results.Ok(teacher);
        }

        private static async Task<IResult> CreateTeacher(CreateTeacherDto dto, SchoolDbContext db)
        {
            var teacher = new Teacher
            {
                Name = dto.Name,
                Department = dto.Department,
                Email = dto.Email,
                Phone = dto.Phone
            };

            if(dto.Id != 0 )
            {
                teacher.Id = dto.Id;
                db.Teachers.Update(teacher);
            }
            else
            {
                db.Teachers.Add(teacher);
            }

            await db.SaveChangesAsync();

            var result = new TeacherDto(teacher.Id, teacher.Name, teacher.Department, teacher.Email, teacher.Phone, teacher.IsActive);
            return Results.Created($"/api/teachers/{teacher.Id}", result);
        }

        // private static async Task<IResult> UpdateTeacher(int id, UpdateTeacherDto dto, SchoolDbContext db)
        // {
        //     var teacher = await db.Teachers.FindAsync(id);
        //     if (teacher is null) return Results.NotFound();

        //     teacher.Name = dto.Name;
        //     teacher.Department = dto.Department;
        //     teacher.Email = dto.Email;
        //     teacher.Phone = dto.Phone;
        //     teacher.IsActive = dto.IsActive;

        //     await db.SaveChangesAsync();
        //     return Results.Ok(new TeacherDto(teacher.Id, teacher.Name, teacher.Department, teacher.Email, teacher.Phone, teacher.IsActive));
        // }

        private static async Task<IResult> DeleteTeacher(int id, SchoolDbContext db)
        {
            var teacher = await db.Teachers.FindAsync(id);
            if (teacher is null) return Results.NotFound();

            db.Teachers.Remove(teacher);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }
    }
}