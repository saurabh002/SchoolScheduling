namespace SchoolScheduling.Dtos;

public record TeacherDto(
    int Id,
    string Name,
    string? Department,
    string? Email,
    string? Phone,
    bool IsActive
);