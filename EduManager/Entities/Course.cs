using Microsoft.EntityFrameworkCore;

namespace EduManager.Entities;
public enum CourseType { Theory, Practice, Lab }
public enum CourseForm { FullTime, PartTime, Combined }
public enum HourType { Weekly, Semesterly }


[Index(nameof(CourseCode), IsUnique = true)]
public class Course
{
    public int Id { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }
    public string Semester { get; set; } = string.Empty;
    public int MaxStudents { get; set; }
    public CourseType Type { get; set; }
    public CourseForm Form { get; set; }
    public HourType HourUnit { get; set; }
    public int Hours { get; set; }
    public ICollection<User> Teachers { get; set; } = new List<User>();
    public ICollection<User> Students { get; set; } = new List<User>();
}