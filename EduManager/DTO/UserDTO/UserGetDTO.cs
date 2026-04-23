namespace EduManager.DTO.UserDto;

public class UserGetDTO
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string StudyMode { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}