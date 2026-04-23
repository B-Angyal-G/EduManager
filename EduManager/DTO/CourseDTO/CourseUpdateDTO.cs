using EduManager.Entities;

namespace EduManager.DTO.CourseDTO;

public class CourseUpdateDTO
{
    public int Id { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string Semester { get; set; } = string.Empty;
    public int MaxStudents { get; set; }
    public CourseType Type { get; set; }
    public CourseForm Form { get; set; }
    public int Hours { get; set; }
    public HourType HourUnit { get; set; }
}