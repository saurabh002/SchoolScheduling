using System.ComponentModel.DataAnnotations;
namespace SchoolScheduling.Dtos{
    public record CreateTeacherDto(
        [Required, MaxLength(100)] string Name,
        [MaxLength(100)] string? Department,
        [EmailAddress, MaxLength(150)] string? Email,
        [Phone, MaxLength(20)] string? Phone,
         List<TimetableRowDto> Schedule  // the rows from "Class Assigned" - can be empty
    );
}