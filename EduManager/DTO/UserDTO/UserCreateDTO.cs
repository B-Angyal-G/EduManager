namespace EduManager.DTO.UserDto;

public class UserCreateDTO
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int Role { get; set; } 
    public int StudyMode { get; set; }
}