using Microsoft.EntityFrameworkCore;

namespace EduManager.Entities;

public enum UserRole { Student, Teacher, Admin }
public enum StudyMode { None, FullTime, PartTime }


[Index(nameof(Email), IsUnique = true)]
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; } 
    public StudyMode StudyMode { get; set; } = StudyMode.None;
    public bool IsActive { get; set; } = true; 
}