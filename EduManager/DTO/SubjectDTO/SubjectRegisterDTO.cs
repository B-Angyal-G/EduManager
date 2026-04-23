namespace EduManager.DTO.SubjectDTO;

public class SubjectRegisterDTO
{
    public int StudentId { get; set; }
    public List<int> CourseIds { get; set; } = new();
}