namespace EduManager.DTO.GradeDTO;

public class GradeCreateDTO
{
    public int SubjectId { get; set; }
    public int StudentId { get; set; }
    public string Semester { get; set; } = string.Empty;
    public int GradeValue { get; set; }
    public DateTime Time { get; set; }
}