namespace EduManager.DTO.SubjectDTO;

public class SubjectGetDTO
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int Credits { get; set; }
    public bool IsActive { get; set; }
}