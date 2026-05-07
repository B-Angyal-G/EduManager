namespace EduManager.DTO.GradeDTO;

public class GradeGetDTO
{
    public int Id { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public int GradeValue { get; set; }
    public DateTime Time { get; set; }
    public string Semester { get; set; } = string.Empty;
}