namespace EduManager.DTO.CourseDTO;

public class CourseChangeDTO
{
    public int StudentId { get; set; }
    public int FromCourseId { get; set; }
    public int ToCourseId { get; set; }
}