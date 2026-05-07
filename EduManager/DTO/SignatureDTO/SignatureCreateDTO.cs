namespace EduManager.DTO.SignatureDTO;

public class SignatureCreateDTO
{
    public int SubjectId { get; set; }
    public int StudentId { get; set; }
    public string Semester { get; set; } = string.Empty;
    public bool Value { get; set; }
    public DateTime Time { get; set; }
}