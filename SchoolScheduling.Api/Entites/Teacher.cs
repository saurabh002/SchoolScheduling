using System.ComponentModel.DataAnnotations;

namespace SchoolScheduling.Entities
{
// Teacher.cs
public class Teacher
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Department { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<TimetableEntry> TimetableEntries { get; set; } = [];
}
}