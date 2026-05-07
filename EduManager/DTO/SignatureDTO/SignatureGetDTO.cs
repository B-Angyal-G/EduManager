namespace EduManager.DTO.SignatureDTO;

public class SignatureGetDTO
{
    public int Id { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string Semester { get; set; } = string.Empty;
    public bool IsSigned { get; set; }
    public DateTime Time { get; set; }
}