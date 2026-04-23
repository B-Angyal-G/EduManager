namespace EduManager.DTO.CourseDTO;

public class CourseGetDTO
{
    public int Id { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string Semester { get; set; } = string.Empty;
    public int MaxStudents { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Form { get; set; } = string.Empty;
    public string HoursDescription { get; set; } = string.Empty;
    public List<string> TeacherNames { get; set; } = new();
}